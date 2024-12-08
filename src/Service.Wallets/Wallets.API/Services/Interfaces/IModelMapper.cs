using Transactions.Contracts.Messages.Accounts;
using Wallets.API.Data.Models;
using Wallets.Contracts.Contracts;

namespace Wallets.API.Services.Interfaces;

public interface IModelMapper
{
    CreateAccountMessage ToCreateAccountMessage(WalletInDb walletInDb);
    WalletResponse ToWalletResponse(WalletInDb walletInDb);
}