using Wallets.Contracts.Contracts;

namespace Wallets.Contracts.Interfaces;

public interface IWalletsApi
{
    Task<CreateWalletResponse> CreateWallet(CreateWalletRequest request);
}