using FluentValidation;
using Lib.MessageBroker.Contracts;
using Microsoft.Extensions.Options;
using Transactions.Contracts.Messages;

namespace Transactions.API.MessageHandling.Consumers;

public class TestMessageConsumer(
    IOptions<ConsumingSettings> _consumingSettings)
    : IConsumer<TestMessage>
{
    private const string QueueName = "transactions-test-message";

    private readonly ConsumingSettings _consumingSettings = _consumingSettings.Value;

    string IConsumerSettings.QueueName => QueueName;

    AckType IConsumerSettings.Acknowledgment => AckType.RepublishOnFailure;
    
    // The * character matches exactly one word in the routing key, while the # character matches zero or more words.
    string IConsumerSettings.RoutingKey => "#";

    ushort IConsumerSettings.QueuePrefetchCount => _consumingSettings.PrefetchCount;

    public async Task Consume(TestMessage message) => Console.WriteLine($"Test message {message.Value}");
}