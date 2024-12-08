namespace MasterOfCoin.API.ApiContracts.Wallet;

public record EditWalletApiRequest(
    Guid WalletId, 
    string? Name, 
    string? Currency, 
    decimal? Value, 
    bool? Cumulative);