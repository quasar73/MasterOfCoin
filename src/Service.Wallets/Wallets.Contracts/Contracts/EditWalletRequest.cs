namespace Wallets.Contracts.Contracts;

public record EditWalletRequest(
    Guid WalletId, 
    string? Name, 
    string? Currency, 
    decimal? Value, 
    bool? Cumulative, 
    Guid SpaceId);