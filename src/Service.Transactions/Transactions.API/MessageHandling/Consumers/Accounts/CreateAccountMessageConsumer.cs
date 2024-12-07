using Lib.MessageBroker.Contracts;
using Microsoft.Extensions.Options;
using Transactions.API.Data.Interfaces;
using Transactions.API.Data.Models;
using Transactions.Contracts.Messages.Accounts;

namespace Transactions.API.MessageHandling.Consumers.Accounts;

public class CreateAccountMessageConsumer(
    IAccountRepository _repository,
    IOptions<ConsumingSettings> _consumingSettings)
    : IConsumer<CreateAccountMessage>
{
    private const string QueueName = "transactions-create-account";

    private readonly ConsumingSettings _consumingSettings = _consumingSettings.Value;

    string IConsumerSettings.QueueName => QueueName;

    AckType IConsumerSettings.Acknowledgment => AckType.RepublishOnFailure;
    
    // The * character matches exactly one word in the routing key, while the # character matches zero or more words.
    string IConsumerSettings.RoutingKey => "#";

    ushort IConsumerSettings.QueuePrefetchCount => _consumingSettings.PrefetchCount;

    public Task Consume(CreateAccountMessage message)
    {
        var accountInDb = new AccountInDb
        {
            Id = message.Id,
            Name = message.Name,
            SpaceId = message.SpaceId
        };

        return _repository.CreateAccount(accountInDb);
    }
}