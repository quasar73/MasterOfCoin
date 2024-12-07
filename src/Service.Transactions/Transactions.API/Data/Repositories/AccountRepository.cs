using Lib.Db;
using Transactions.API.Data.Interfaces;
using Transactions.API.Data.Models;

namespace Transactions.API.Data.Repositories;

public class AccountRepository(IDatabase _database) : IAccountRepository
{
    public Task CreateAccount(AccountInDb accountInDb) => _database.Execute(
        "INSERT INTO accounts(id, name, space_id)" +
        "VALUES (@Id, @Name, @SpaceId)", accountInDb);
}