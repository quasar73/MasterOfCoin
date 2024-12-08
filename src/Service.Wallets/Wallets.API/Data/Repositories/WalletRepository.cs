using Lib.Db;
using Wallets.API.Data.Interfaces;
using Wallets.API.Data.Models;

namespace Wallets.API.Data.Repositories;

public class WalletRepository(IDatabase _database) : IWalletRepository
{
    public Task<int> Create(WalletInDb walletInDb) => _database.Execute(
        "INSERT INTO wallets(id, name, currency, value, cumulative, space_id, account_id, archived)" +
        "VALUES (@Id, @Name, @Currency, @Value, @Cumulative, @SpaceId, @AccountId, @Archived)", walletInDb);

    public Task<int> Update(WalletInDb walletInDb) => _database.Execute(
        "UPDATE wallets SET name = @Name, currency = @Currency, cumulative = @Cumulative, value = @Value, archived = @Archived", walletInDb);

    public Task<WalletInDb> Find(Guid walletId, Guid spaceId) =>
        _database.GetOrDefault<WalletInDb>("SELECT * FROM wallets WHERE id = @walletId AND space_id = @spaceId", new { walletId, spaceId });

    public Task<List<WalletInDb>> GetList(Guid spaceId) =>
        _database.GetList<WalletInDb>("SELECT * FROM wallets WHERE space_id = @spaceId", new { spaceId });
}