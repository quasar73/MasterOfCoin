using Lib.Db;
using MasterOfCoin.API.Data.Interfaces;
using MasterOfCoin.API.Data.Models;

namespace MasterOfCoin.API.Data.Repositories;

public class UserRepository(IDatabase _database) : IUserRepository
{
    public async Task<UserInDb?> AuthorizeInDb(string username, string password)
    {
        var userInDb = await _database.GetOrDefault<UserInDb>("SELECT * FROM users WHERE username = LOWER(@username)", new { username });

        if (userInDb is null) return null;
        
        // password hashing

        if (userInDb.PasswordHash == password) return userInDb;

        return null;
    }
}