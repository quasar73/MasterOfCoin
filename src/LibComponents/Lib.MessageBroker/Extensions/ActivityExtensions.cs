using Lib.MessageBroker.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;

namespace Lib.MessageBroker.Extensions;

internal static class ActivityExtensions
{
    internal static void TraceCorrelationHeaderId(this Activity? activity, IBasicProperties basicProperties)
    {
        if (activity is null)
        {
            return;
        }

        basicProperties.Headers.TryGetValue(HardcodedConstants.correlationHeaderId, out var correlationId);
        if (correlationId is byte[] correlationBytes)
        {
            var correlationIdString = Encoding.UTF8.GetString(correlationBytes);
            activity?.SetTag(HardcodedConstants.correlationHeaderId, correlationIdString);
        }
    }

    internal static void TraceError<TMessage>(this Activity? activity, TMessage message, ulong deliveryTag, AckType ackType)
    {
        activity?.AddEvent(new("message-handle-error", tags: new()
        {
            { nameof(message), message },
            { nameof(BasicDeliverEventArgs.DeliveryTag), deliveryTag },
            { "AcknowledgmentType", ackType },
        }));
    }

    internal static void TraceException(this Activity? activity, Exception exception)
    {
        activity?.AddEvent(new("message-handle-failed", tags: new()
        {
            { "Error", exception.Message },
            { "StackTrace", exception.StackTrace },
        }));
    }
}
