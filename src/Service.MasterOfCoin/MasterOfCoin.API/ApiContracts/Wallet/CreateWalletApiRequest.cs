namespace MasterOfCoin.API.ApiContracts.Wallet;

public record CreateWalletApiRequest(string Name, decimal InitialValue, string Currency, bool Cumulative);