using Wallets.API.Data.Models;

namespace Wallets.API.Data.Interfaces;

public interface IWalletRepository
{
    Task CreateWallet(WalletInDb walletInDb);
}