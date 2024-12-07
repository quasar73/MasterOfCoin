namespace Lib.MessageBroker.Contracts
{
    public interface IConsumerSettings
    {
        /// <summary>
        /// The name of the queue to listen to. Default equals to full message type name.
        /// </summary>
        string QueueName { get; }

        /// <summary>
        /// The name of the routing key to listen to. Default equals to queue name.
        /// </summary>
        string RoutingKey { get; }

        /// <summary>
        /// The name of the exchange to connect queue to. Default equals to full message type name.
        /// </summary>
        string ExchangeName { get; }

        /// <summary>
        /// Acknowledgement type for message handling. Default is <code>AckType.AckOnFailure</code>.
        /// </summary>
        AckType Acknowledgment => AckType.AckOnFailure;

        /// <summary>
        /// Should queue be persistent or be deleted after disconnection. Default is persistent.
        /// </summary>
        bool QueueIsPersistent => true;

        /// <summary>
        /// How many messages can be consumed at once by this consumer. Default is 1.
        /// </summary>
        ushort QueuePrefetchCount => 1;

        /// <summary>
        /// Force sequential consuming by only one consumer. Default is false.
        /// </summary>
        bool QueueSingleComsumer => false;

        /// <summary>
        /// If set to <value>true</value>, do not declare any message broker assets like queues. Default is <value>false</value>.
        /// </summary>
        bool RuntimeSkipInitialization => false;
    }
}
