using MasterOfCoin.API.Data.Models;

namespace MasterOfCoin.API.Data.Interfaces;

public interface ISpaceRepository
{
    Task Create(SpaceInDb spaceInDb);
}