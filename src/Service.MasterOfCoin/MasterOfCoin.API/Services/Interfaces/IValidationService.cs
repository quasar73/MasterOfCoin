namespace MasterOfCoin.API.Services.Interfaces;

public interface IValidationService
{
    Task<bool> ValidateUserSpace(string username, Guid spaceId);
}