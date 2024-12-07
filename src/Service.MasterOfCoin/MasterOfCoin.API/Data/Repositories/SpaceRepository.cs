using Lib.Db;
using MasterOfCoin.API.Data.Interfaces;
using MasterOfCoin.API.Data.Models;

namespace MasterOfCoin.API.Data.Repositories;

public class SpaceRepository(IDatabase _database) : ISpaceRepository
{
    public Task Create(SpaceInDb spaceInDb) => _database.Execute(
        "INSERT INTO spaces (id, name, user_id, deleted) " +
        "VALUES(@Id, @Name, @UserId, false)", spaceInDb);

    public Task MarkAsDeleted(Guid spaceId, Guid userId) =>
        _database.Execute("UPDATE spaces SET deleted = true WHERE id = @spaceId AND user_id = @userId", new { spaceId, userId });

    public Task<List<SpaceInDb>> GetListByUseId(Guid userId) =>
        _database.GetList<SpaceInDb>("SELECT * FROM spaces WHERE user_id = @userId AND deleted = false", new { userId });

}