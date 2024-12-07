using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace Lib.MessageBroker.Extensions;

internal static class ModelExtensions
{
    private static readonly ConcurrentDictionary<string, string> _exchangeCache = new();

    internal static void BasicNackIfOpen(this IModel channel, ulong deliveryTag, bool multiple, bool requeue)
    {
        if (channel.IsOpen)
        {
            channel.BasicNack(deliveryTag, multiple, requeue);
        }
    }

    internal static void ExchangeDeclareCached(this IModel channel, string exchange, string type)
    {
        if (_exchangeCache.TryGetValue(exchange, out var _))
        {
            return;
        }

        channel.ExchangeDeclare(exchange, type, default, default, default);
        _exchangeCache.TryAdd(exchange, type);
    }
}
