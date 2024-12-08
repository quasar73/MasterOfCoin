using Wallets.Contracts.Contracts.Enums;

namespace Wallets.Contracts.Contracts;

public record WalletsListResponse(RequestStatus Status, WalletResponse[] Wallets);