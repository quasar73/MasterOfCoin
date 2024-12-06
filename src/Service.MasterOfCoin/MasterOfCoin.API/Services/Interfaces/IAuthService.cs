using MasterOfCoin.API.Services.Models;

namespace MasterOfCoin.API.Services.Interfaces;

public interface IAuthService
{
    public Task<LoginState> Authorize(string username, string password);
    public Task<LoginState> Refresh(string refreshToken);
    public Task InvalidateToken(string token);
    public Task<RegisterStatus> Register(RegisterInfo info);
}