using Lib.Db;
using Transactions.API.Data.Interfaces;
using Transactions.API.Data.Models;

namespace Transactions.API.Data.Repositories;

public class AccountRepository(IDatabase _database) : IAccountRepository
{
    public Task CreateAccount(AccountInDb accountInDb) => _database.Execute(
        "INSERT INTO accounts(id, type, space_id)" +
        "VALUES (@Id, @Type, @SpaceId)", accountInDb);

    public Task UpdateAccount(Guid id, string name) => _database.Execute(
        "UPDATE accounts SET name = @name WHERE id = @id", new { name, id });
}