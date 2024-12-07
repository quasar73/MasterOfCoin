using Lib.Db;
using Wallets.API.Data.Interfaces;
using Wallets.API.Data.Models;

namespace Wallets.API.Data.Repositories;

public class WalletRepository(IDatabase _database) : IWalletRepository
{
    public Task CreateWallet(WalletInDb walletInDb) => _database.Execute(
        "INSERT INTO wallets(id, name, currency, value, cumulative, space_id, account_id)" +
        "VALUES (@Id, @Name, @Currency, @Value, @Cumulative, @SpaceId, @AccountId)", walletInDb);
}