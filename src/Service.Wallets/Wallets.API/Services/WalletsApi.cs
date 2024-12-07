using Lib.MessageBroker.Contracts;
using Transactions.Contracts.Messages.Accounts;
using Wallets.API.Data.Interfaces;
using Wallets.API.Data.Models;
using Wallets.Contracts.Contracts;
using Wallets.Contracts.Interfaces;

namespace Wallets.API.Services;

public class WalletsApi(IWalletRepository _repository, IPublisher _publisher) : IWalletsApi
{
    public async Task<CreateWalletResponse> CreateWallet(CreateWalletRequest request)
    {
        var walletInDb = new WalletInDb
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Currency = request.Currency,
            SpaceId = request.SpaceId,
            Value = request.InitialValue,
            Cumulative = request.Cumulative,
            AccountId = Guid.NewGuid()
        };
        await _repository.CreateWallet(walletInDb);
        
        await _publisher.Publish(new CreateAccountMessage(walletInDb.AccountId, walletInDb.Name, walletInDb.SpaceId));

        return new(new("Ok"), walletInDb.Id);
    }
}