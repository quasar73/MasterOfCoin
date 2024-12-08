using Wallets.API.Data.Models;

namespace Wallets.API.Data.Interfaces;

public interface IWalletRepository
{
    Task<int> CreateWallet(WalletInDb walletInDb);
    Task<int> EditWallet(WalletInDb walletInDb);
    Task<WalletInDb?> Find(Guid walletId, Guid spaceId);
    Task<List<WalletInDb>> GetList(Guid spaceId);
}