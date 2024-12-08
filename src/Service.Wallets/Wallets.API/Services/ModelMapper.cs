using Transactions.Contracts.Messages;
using Transactions.Contracts.Messages.Accounts;
using Wallets.API.Data.Models;
using Wallets.API.Services.Interfaces;
using Wallets.Contracts.Contracts;

namespace Wallets.API.Services;

public class ModelMapper : IModelMapper
{
    public CreateAccountMessage ToCreateAccountMessage(WalletInDb walletInDb) => new(
        walletInDb.AccountId,
        walletInDb.SpaceId,
        AccountCreatingSource.Wallet);

    public WalletResponse ToWalletResponse(WalletInDb walletInDb) => new(
        walletInDb.Id,
        walletInDb.Name,
        walletInDb.Currency,
        walletInDb.Value,
        walletInDb.Cumulative);
}