namespace Wallets.Contracts.Contracts;

public record WalletResponse(Guid Id, string Name, string Currency, decimal Value, bool Cumulative);