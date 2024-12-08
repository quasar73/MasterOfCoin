using Wallets.Contracts.Contracts;

namespace Wallets.Contracts.Interfaces;

public interface IWalletsApi
{
    Task<CreateWalletResponse> CreateWallet(CreateWalletRequest request);
    Task<StatusResponse> EditWallet(EditWalletRequest request);
    Task<StatusResponse> ArchiveWallet(ArchiveWalletRequest request);
    Task<WalletsListResponse> GetWallets(GetWalletsRequest request);
    Task<GetWalletResponse> GetWallet(GetWalletRequest request);
}