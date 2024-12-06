using Lib.Db;
using MasterOfCoin.API.Data.Interfaces;
using MasterOfCoin.API.Data.Models;

namespace MasterOfCoin.API.Data.Repositories;

public class SpaceRepository(IDatabase _database) : ISpaceRepository
{
    public Task Create(SpaceInDb spaceInDb) => _database.Execute(
        "INSERT INTO spaces (id, name, user_id, deleted) " +
        "VALUES(@Id, @Name, @UserId, false)", spaceInDb);
}