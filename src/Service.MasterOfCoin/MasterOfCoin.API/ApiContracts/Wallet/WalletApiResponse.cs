namespace MasterOfCoin.API.ApiContracts.Wallet;

public record WalletApiResponse(Guid Id, string Name, string Currency, decimal Value, bool Cumulative);