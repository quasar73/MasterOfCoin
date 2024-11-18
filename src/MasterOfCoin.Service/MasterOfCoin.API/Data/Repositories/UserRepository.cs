using Lib.Db;
using MasterOfCoin.API.Data.Interfaces;
using MasterOfCoin.API.Data.Models;
using MasterOfCoin.API.Extensions;

namespace MasterOfCoin.API.Data.Repositories;

public class UserRepository(IDatabase _database) : IUserRepository
{
    public async Task<UserInDb?> AuthorizeInDb(string username, string password)
    {
        var userInDb = await _database.GetOrDefault<UserInDb>("SELECT * FROM users WHERE username = LOWER(@username)", new { username });

        if (userInDb is null) return null;

        var hash = password.CalculatePasswordHash(userInDb.PasswordSalt);
        if (userInDb.PasswordHash == hash) return userInDb;

        return null;
    }

    public Task Create(UserInDb userInDb) => _database.Execute(
        "INSERT INTO users (id, username, password_hash, password_salt, displayed_name, email, avatar) " +
        "VALUES(@Id, @Username, @PasswordHash, @PasswordSalt, @DisplayedName, @Email, @Avatar)", userInDb);

    public async Task<UserInDb?> GetById(Guid id)
    {
        return await _database.GetOrDefault<UserInDb>("SELECT * FROM users WHERE id = @id", new { id });
    }
}