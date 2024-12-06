namespace MasterOfCoin.API.ApiContracts.Auth;

public record RegisterRequest(string Username, string Password, string DisplayedName, string Email);