using MasterOfCoin.API.Data.Models;

namespace MasterOfCoin.API.Services.Interfaces;

public interface ISpaceService
{
    Task<SpaceInDb> CreateSpace(string name, string username);
    Task DeleteSpace(Guid spaceId, string username);
    Task<SpaceInDb[]> GetList(string username);
}