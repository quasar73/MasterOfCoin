using Lib.MessageBroker.Contracts;
using Lib.MessageBroker.Extensions;
using Lib.MessageBroker.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Lib.MessageBroker.Services;

public sealed class MessageBrokerSubscription : IDisposable
{
    private readonly IConnection _connection;
    private readonly IServiceProvider _services;
    private readonly ILogger<MessageBrokerSubscription> _logger;
    private readonly MethodInfo _subscribeConsumerMethod;
    private readonly ConcurrentBag<AsyncEventingBasicConsumer> _consumersCollection;
    private readonly ActivitySourceHolder _activitySourceHolder;

    public MessageBrokerSubscription(
        Type[] consumers,
        IConnection connection,
        IServiceProvider services,
        ILogger<MessageBrokerSubscription> logger,
        ActivitySourceHolder activitySourceHolder)
    {
        _connection = connection;
        _services = services;
        _logger = logger;
        _activitySourceHolder = activitySourceHolder;

        _subscribeConsumerMethod = GetType().GetMethod(nameof(this.SubscribeConsumer), BindingFlags.NonPublic | BindingFlags.Instance)!;
        _consumersCollection = [];

        var declarations = consumers
            .SelectMany(t => GetDeclarations(t).ToDictionary(i => i, _ => t))
            .GroupBy(x => x.Key, x => x.Value);

        SubscribeConsumers(declarations);
    }

    private static Type[] GetDeclarations(Type type)
    {
        return type.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumer<>))
            .ToArray();
    }

    private void SubscribeConsumers(IEnumerable<IGrouping<Type, Type>> declarations)
    {
        foreach (var declaration in declarations)
        {
            foreach (var consumer in declaration)
            {
                var messageType = declaration.Key.GetGenericArguments()[0];
                var regMethod = _subscribeConsumerMethod.MakeGenericMethod(consumer, messageType);
                if (regMethod.Invoke(this, []) is AsyncEventingBasicConsumer consumerObject)
                {
                    _consumersCollection.Add(consumerObject);
                }
            }
        }
    }

    private AsyncEventingBasicConsumer? SubscribeConsumer<TConsumer, TMessage>() 
        where TConsumer : class, IConsumerSettings, IConsumer<TMessage>
        where TMessage : class
    {
        var handler = _services.GetRequiredService<TConsumer>();
        var publisher = _services.GetRequiredService<IPublisher>();
        var consumerType = typeof(TConsumer).FullName;

        if (handler.RuntimeSkipInitialization)
        {
            return null;
        }

        try
        {
            return Subscribe(handler, publisher, GetSafeAsyncMessageHandler(handler));
        }
        catch (Exception ex)
        {
            var queueName = handler.QueueName;
            _logger.LogError(ex, "Unable to subscribe the {consumerType} message handler on the {queueName} queue", consumerType, queueName);
            return null;
        }
    }

    private AsyncEventingBasicConsumer Subscribe<TMessage>(IConsumerSettings consumerSettings, IPublisher publisher, Func<TMessage, Task<bool>> safeHandler)
        where TMessage : class
    {
        var channel = Connect(consumerSettings);

        var eventingBasicConsumer = new AsyncEventingBasicConsumer(channel);
        var activitySourceHolder = _activitySourceHolder;
        var logger = _logger;

        eventingBasicConsumer.Received += async (_, ea) => /* parallelize handling of messages*/ await Task.Factory.StartNew(async () =>
        {
            using var activity = activitySourceHolder.ActivitySource?.StartActivity(nameof(eventingBasicConsumer.Received));
            activity.TraceCorrelationHeaderId(ea.BasicProperties);

            try
            {
                var message = BodyToMessage<TMessage>(ea.Body);
                activity?.SetTag("messageType", typeof(TMessage).FullName);

                using var handleActivity = activitySourceHolder.ActivitySource?.StartActivity("MassageHandle");
                handleActivity?.SetTag("consumerType", consumerSettings.GetType().FullName);

                if (await safeHandler(message))
                {
                    channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                else
                {
                    handleActivity.TraceError(message, ea.DeliveryTag, consumerSettings.Acknowledgment);

                    switch (consumerSettings.Acknowledgment)
                    {
                        case AckType.AckOnlyOnSuccess:
                            channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                            break;
                        case AckType.AckOnFailure:
                            channel.BasicAck(ea.DeliveryTag, multiple: false);
                            break;
                        case AckType.RequeueOnFailure:
                            channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                            break;
                        case AckType.RepublishOnFailure:
                            await publisher.Publish(message, consumerSettings.RoutingKey);
                            channel.BasicAck(ea.DeliveryTag, multiple: false);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while processing a message from Rabbit");
                activity.TraceException(ex);
             
                channel.BasicNackIfOpen(ea.DeliveryTag, multiple: false, requeue: true);
            }
        });
        eventingBasicConsumer.ConsumerCancelled += (_, e) =>
        {
            var tags = string.Join(",", e.ConsumerTags);
            logger.LogWarning("consumer.Cancelled. ConsumerTags: {tags}", tags);
            return Task.CompletedTask;
        };
        eventingBasicConsumer.Shutdown += (_, e) =>
        {
            var replyText = e.ReplyText;
            var initiator = e.Initiator;
            logger.LogWarning("consumer.Shutdown. ReplayText: {replyText}, Initiator: {initiator}", replyText, initiator);
            return Task.CompletedTask;
        };
        eventingBasicConsumer.Unregistered += (_, e) =>
        {
            var tags = string.Join(",", e.ConsumerTags);
            logger.LogWarning("consumer.Unregistered. ConsumerTag: {tags}", tags);
            return Task.CompletedTask;
        };
        channel.CallbackException += (_, e) => logger.LogError(e.Exception, "Received channel.CallbackException");
        channel.ModelShutdown += (_, e) =>
        {
            var replyText = e.ReplyText;
            var initiator = e.Initiator;
            logger.LogWarning("channel.ModelShutdown. ReplayText: {replyText}, Initiator: {initiator}", replyText, initiator);
        };

        channel.BasicConsume(consumerSettings.QueueName, false, eventingBasicConsumer);
        return eventingBasicConsumer;
    }

    private IModel Connect(IConsumerSettings consumerSettings)
    {
        var channel = _connection.CreateModel();
        channel.ExchangeDeclare(consumerSettings.ExchangeName, HardcodedConstants.exchangeType);

        Dictionary<string, object> arguments = default!;
        if (consumerSettings.QueueSingleComsumer)
        {
            arguments = new Dictionary<string, object> { { "x-single-active-consumer", true } };
        }
        channel.QueueDeclare(consumerSettings.QueueName, consumerSettings.QueueIsPersistent, false, !consumerSettings.QueueIsPersistent, arguments);
        channel.QueueBind(consumerSettings.QueueName, consumerSettings.ExchangeName, HardcodedConstants.AnyRountingKey);

        var actualRoutingKey = consumerSettings.RoutingKey.Length > 255 ? consumerSettings.RoutingKey[^255..] : consumerSettings.RoutingKey;
        channel.QueueBind(consumerSettings.QueueName, consumerSettings.ExchangeName, actualRoutingKey);

        if (consumerSettings.QueuePrefetchCount > 0)
        {
            channel.BasicQos(0u, consumerSettings.QueuePrefetchCount, global: false);
        }

        return channel;
    }

    private static TMessage BodyToMessage<TMessage>(ReadOnlyMemory<byte> body) 
        => typeof(TMessage) == typeof(byte[]) ? (TMessage)(object)body : JsonSerializer.Deserialize<TMessage>(Encoding.UTF8.GetString(body.ToArray()))!;

    private Func<TMessage, Task<bool>> GetSafeAsyncMessageHandler<TMessage>(IConsumer<TMessage> handler) where TMessage : class
    {
        return async message =>
        {
            try
            {
                await handler.Consume(message);
                return true;
            }
            catch (Exception ex)
            {
                var messageContent = JsonSerializer.Serialize(message);
                var messageType = message.GetType().FullName;
                var consumerType = handler.GetType().FullName;
                _logger.LogCritical(ex, "Message consume failed in {consumerType}, message {messageType}: {messageContent}", consumerType, messageType, messageContent);
                return false;
            }
        };
    }

    public void Dispose()
    {
        _consumersCollection.AsParallel().ForAll(x => x.Model.Dispose());
        _consumersCollection.Clear();
    }
}
