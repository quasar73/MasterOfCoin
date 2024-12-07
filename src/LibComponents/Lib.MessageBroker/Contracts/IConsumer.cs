namespace Lib.MessageBroker.Contracts
{
    public interface IConsumer<T> : IConsumerSettings where T : class
    {
        /// <summary>
        /// Receive the message from thie queue and handle it
        /// </summary>
        /// <param name="message">Message itself</param>
        /// <returns></returns>
        Task Consume(T message);

        /// <summary>
        /// The name of the queue to listen to. Default equals to full message type name.
        /// </summary>
        string IConsumerSettings.QueueName => typeof(T).FullName!;

        /// <summary>
        /// The name of the routing key to listen to. Default equals to queue name.
        /// </summary>
        string IConsumerSettings.RoutingKey => QueueName;

        /// <summary>
        /// The name of the exchange to connect queue to. Default equals to full message type name.
        /// </summary>
        string IConsumerSettings.ExchangeName => typeof(T).FullName!;

    }
}
