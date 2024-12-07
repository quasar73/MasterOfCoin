using MasterOfCoin.API.Data.Models;

namespace MasterOfCoin.API.Data.Interfaces;

public interface IUserRepository
{
    Task<UserInDb?> AuthorizeInDb(string username, string password);
    Task Create(UserInDb userInDb);
    Task<UserInDb?> GetById(Guid id);
    Task<UserInDb?> GetByUsername(string username);
}