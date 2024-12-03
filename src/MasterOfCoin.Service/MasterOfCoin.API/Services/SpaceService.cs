using MasterOfCoin.API.Data.Interfaces;
using MasterOfCoin.API.Data.Models;
using MasterOfCoin.API.Services.Interfaces;

namespace MasterOfCoin.API.Services;

public class SpaceService(ISpaceRepository _repository) : ISpaceService
{
    public Task CreateSpace(string name, Guid userId)
    {
        var spaceInDb = new SpaceInDb
        {
            Id = Guid.NewGuid(),
            Name = name,
            UserId = userId,
            Deleted = false
        };

        return _repository.Create(spaceInDb);
    }
}