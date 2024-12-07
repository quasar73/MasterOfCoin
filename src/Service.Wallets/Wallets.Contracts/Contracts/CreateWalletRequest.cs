namespace Wallets.Contracts.Contracts;

public record CreateWalletRequest(string Name, Guid SpaceId, decimal InitialValue, string Currency, bool Cumulative);