using MasterOfCoin.API.Data.Models;

namespace MasterOfCoin.API.Data.Interfaces;

public interface IUserRepository
{
    public Task<UserInDb?> AuthorizeInDb(string username, string password);
    public Task Create(UserInDb userInDb);
}