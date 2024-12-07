namespace Lib.MessageBroker.Contracts
{
    public interface IPublisher
    {
        /// <summary>
        /// Async Publish message
        /// <paramref name="message">Message to publish</paramref>
        /// <paramref name="rountingKey">Queue to publish to. Publish to all subscribed queues if null.
        /// </summary>
        Task Publish<T>(T message, string? rountingKey = default) where T : class;

        /// <summary>
        /// Async Publish message without acknowledgements
        /// <paramref name="message">Message to publish</paramref>
        /// <paramref name="rountingKey">Queue to publish to. Publish to all subscribed queues if null.
        /// </summary>
        Task FireAndForget<T>(T message, string? rountingKey = default) where T : class;
    }
}
