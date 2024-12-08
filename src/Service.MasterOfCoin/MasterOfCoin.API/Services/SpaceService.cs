using MasterOfCoin.API.Data.Interfaces;
using MasterOfCoin.API.Data.Models;
using MasterOfCoin.API.Services.Interfaces;

namespace MasterOfCoin.API.Services;

public class SpaceService(
    ISpaceRepository _repository, 
    IUserRepository _userRepository) : ISpaceService
{
    public async Task<SpaceInDb> CreateSpace(string name, string username)
    {
        var user = await GetUserOrThrow(username);
        var spaceInDb = new SpaceInDb
        {
            Id = Guid.NewGuid(),
            Name = name,
            UserId = user.Id,
            Deleted = false
        };

        await _repository.Create(spaceInDb);

        return spaceInDb;
    }

    public async Task DeleteSpace(Guid spaceId, string username)
    {
        var user = await GetUserOrThrow(username);
        await _repository.MarkAsDeleted(spaceId, user.Id);
    }

    public async Task<SpaceInDb[]> GetList(string username)
    {
        var user = await GetUserOrThrow(username);
        return (await _repository.GetListByUseId(user.Id)).ToArray();
    }

    private async Task<UserInDb> GetUserOrThrow(string username)
    {
        var user = await _userRepository.GetByUsername(username);

        if (user is null)
        {
            throw new InvalidDataException($"User with name '{username}' not found.");
        }

        return user;
    }
}