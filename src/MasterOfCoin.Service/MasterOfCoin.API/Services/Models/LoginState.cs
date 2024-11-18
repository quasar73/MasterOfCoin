namespace MasterOfCoin.API.Services.Models;

public record LoginState(LoginStatus Status, string Token, string RefreshToken);