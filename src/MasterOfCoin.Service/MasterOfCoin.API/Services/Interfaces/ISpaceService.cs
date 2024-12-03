namespace MasterOfCoin.API.Services.Interfaces;

public interface ISpaceService
{
    Task CreateSpace(string name, Guid userId);
}