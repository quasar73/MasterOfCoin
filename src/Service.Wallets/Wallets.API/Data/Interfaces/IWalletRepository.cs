using Wallets.API.Data.Models;

namespace Wallets.API.Data.Interfaces;

public interface IWalletRepository
{
    Task<int> Create(WalletInDb walletInDb);
    Task<int> Update(WalletInDb walletInDb);
    Task<WalletInDb?> Find(Guid walletId, Guid spaceId);
    Task<List<WalletInDb>> GetList(Guid spaceId);
}