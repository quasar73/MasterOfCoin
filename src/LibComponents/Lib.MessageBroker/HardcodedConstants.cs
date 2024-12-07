using RabbitMQ.Client;

namespace Lib.MessageBroker
{
    internal static class HardcodedConstants
    {
        public const string AnyRountingKey = "any";

        // We can parametrize these and other possible parameters later in overrided methods.
        public const string exchangeType = ExchangeType.Topic;
        public static readonly IDictionary<string, object> exchangeHeaders = new Dictionary<string, object>();
        public const string exchangeContentType = "application/json";
        public const bool exchangePersistent = false;
        public const string correlationHeaderId = "correlation-header-id";
    }
}
