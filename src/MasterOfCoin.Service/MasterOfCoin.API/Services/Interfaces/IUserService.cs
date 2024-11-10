using MasterOfCoin.API.Services.Models;

namespace MasterOfCoin.API.Services.Interfaces;

public interface IUserService
{
    public Task<LoginState> Authorize(string username, string password);
}