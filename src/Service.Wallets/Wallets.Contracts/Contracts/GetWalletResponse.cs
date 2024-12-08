using Wallets.Contracts.Contracts.Enums;

namespace Wallets.Contracts.Contracts;

public record GetWalletResponse(RequestStatus Status, WalletResponse? Wallet);