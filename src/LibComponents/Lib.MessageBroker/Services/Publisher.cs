using Lib.MessageBroker.Contracts;
using Lib.MessageBroker.Extensions;
using Lib.MessageBroker.Tracing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Lib.MessageBroker.Services;

public class Publisher(
    ObjectPool<IModel> _pool,
    ILogger<Publisher> _logger,
    ActivitySourceHolder _activitySourceHolder) : IPublisher
{
    public Task Publish<T>(T message, string? rountingKey = null) where T : class
    {
        using var activity = StartActivity(nameof(Publish));
        activity?.SetTag(nameof(rountingKey), rountingKey);
        activity?.SetTag("messageType", typeof(T).FullName);

        var createModelActivity = StartActivity(nameof(IConnection.CreateModel));
        var channel = _pool.Get();
        createModelActivity?.Dispose();

        try
        {
            PublishInner(channel, message, rountingKey, waitForConfirmation: true);
        }
        finally
        {
            _pool.Return(channel);
        }

        return Task.CompletedTask;
    }

    public Task FireAndForget<T>(T message, string? rountingKey = null) where T : class
    {
        using var activity = StartActivity(nameof(FireAndForget));
        activity?.SetTag(nameof(rountingKey), rountingKey);
        activity?.SetTag("messageType", typeof(T).FullName);

        var createModelActivity = StartActivity(nameof(IConnection.CreateModel));
        var channel = _pool.Get();
        createModelActivity?.Dispose();

        try
        {
            PublishInner(channel, message, rountingKey, waitForConfirmation: false);
        }
        finally
        {
            _pool.Return(channel);
        }

        return Task.CompletedTask;
    }

    private void PublishInner<T>(IModel model, T message, string? rountingKey, bool waitForConfirmation) where T : class
    {
        var correlationId = Guid.NewGuid().ToString();

        if (waitForConfirmation)
        {
            //1. Enable publisher acknowledgements if needed
            ConfirmSelect(model);
        }

        //2. Declare an exchange
        var exchangeName = ExchangeDeclare(model, message, correlationId);

        //3. Construct a content header
        var basicProperties = CreateBasicProperties(model, correlationId);

        //4. Publish a message
        BasicPublish(model, message, exchangeName, rountingKey, basicProperties);

        if (waitForConfirmation)
        {
            //5. Wait until all published messages on this channel have been confirmed
            WaitForConfirmsOrDie(model, message, exchangeName);
        }
    }

    private void ConfirmSelect(IModel channel)
    {
        using var confirmSelectActivity = StartActivity(nameof(IModel.ConfirmSelect));
        channel.ConfirmSelect();
    }

    private string ExchangeDeclare<T>(IModel channel, T message, string correlationId)
    {
        using var exchangeDeclareActivity = StartActivity(nameof(IModel.ExchangeDeclare));

        var messageType = CheckMessage(message);
        var exchangeName = messageType.FullName!;

        exchangeDeclareActivity?.SetTag(nameof(exchangeName), exchangeName);
        exchangeDeclareActivity?.SetTag(HardcodedConstants.correlationHeaderId, correlationId);

        channel.ExchangeDeclareCached(exchangeName, HardcodedConstants.exchangeType);

        return exchangeName;
    }

    private IBasicProperties CreateBasicProperties(IModel channel, string correlationId)
    {
        using var createBasicPropertiesActivity = StartActivity(nameof(IModel.CreateBasicProperties));

        var basicProperties = channel.CreateBasicProperties();
        basicProperties.Persistent = HardcodedConstants.exchangePersistent;
        basicProperties.ContentType = HardcodedConstants.exchangeContentType;
        basicProperties.Headers = new Dictionary<string, object>() { { HardcodedConstants.correlationHeaderId, correlationId } };
        if (HardcodedConstants.exchangeHeaders != null && HardcodedConstants.exchangeHeaders.Any())
        {
            foreach (var header in HardcodedConstants.exchangeHeaders)
            {
                basicProperties.Headers.Add(header);
            }
        }

        return basicProperties;
    }

    private void BasicPublish<T>(IModel channel, T message, string exchange, string? rountingKey, IBasicProperties basicProperties)
    {
        using var publishActivity = StartActivity(nameof(IModel.BasicPublish));

        var body = ObjectToByteArray(message, out var textMessage);
        var actualRountingKey = GetActualRoutingKey(rountingKey);

        publishActivity?.SetTag(nameof(rountingKey), actualRountingKey);
        publishActivity?.SetTag(nameof(basicProperties.Headers), basicProperties.Headers);
        publishActivity?.SetTag(nameof(body), body);
        publishActivity?.SetTag(nameof(textMessage), textMessage);

        channel.BasicPublish(exchange, actualRountingKey, basicProperties, body);
    }

    private void WaitForConfirmsOrDie<T>(IModel channel, T message, string exchange)
    {
        using var waitForConfirmsOrDieActivity = StartActivity(nameof(IModel.WaitForConfirmsOrDie));

        try
        {
            // Might be reengeneered to async manner using model.BasicAcks and model.BasicNacks.
            // see https://www.rabbitmq.com/tutorials/tutorial-seven-dotnet.html.
            channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
        }
        catch (Exception ex)
        {
            var messageTypeName = exchange;
            var messageContent = JsonSerializer.Serialize(message);
            _logger.LogError(ex, "Message was not published. {messageTypeName}: {messageContent}", messageTypeName, messageContent);

            waitForConfirmsOrDieActivity?.AddEvent(new ActivityEvent("confirmation-error", tags: new()
            {
                { nameof(messageTypeName), messageTypeName },
                { nameof(messageContent), messageContent },
            }));
        }
    }

    private Activity? StartActivity(string activityName)
        => _activitySourceHolder.ActivitySource?.StartActivity(activityName);

    private static Type CheckMessage<T>(T message)
    {
        if (message == null)
        {
            throw new InvalidOperationException("Message shoult not be null");
        }

        var type = typeof(T);

        if (!type.IsVisible || type.IsConstructedGenericType)
        {
            throw new InvalidOperationException("Dynamic types are not allowed to publish");
        }

        return type;
    }

    private static byte[] ObjectToByteArray<T>(T obj, out string textBody)
    {
        if (obj is byte[] v)
        {
            textBody = "[" + string.Join(",", v) + "]";
            return v;
        }
        textBody = JsonSerializer.Serialize(obj);
        return Encoding.UTF8.GetBytes(textBody);
    }

    private string GetActualRoutingKey(string? rountingKey)
    {
        var actualRountingKey = string.IsNullOrWhiteSpace(rountingKey) ? HardcodedConstants.AnyRountingKey : rountingKey;

        if (actualRountingKey.Length > 255)
        {
            var newKey = actualRountingKey[^255..];
            _logger.LogWarning("Too long queue name {routingKey} on publish, will be used {newKey}", actualRountingKey, newKey);
            actualRountingKey = newKey;
        }

        return actualRountingKey;
    }
}
