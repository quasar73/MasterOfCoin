namespace MasterOfCoin.API.ApiContracts.Wallet;

public record CreateWalletRequest(string Name, decimal InitialValue, string Currency, Guid SpaceId, bool Cumulative);