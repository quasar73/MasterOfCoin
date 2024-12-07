using MasterOfCoin.API.Data.Interfaces;
using MasterOfCoin.API.Services.Interfaces;

namespace MasterOfCoin.API.Services;

public class ValidationService(IUserRepository _userRepository, ISpaceRepository _spaceRepository) : IValidationService
{
    public async Task<bool> ValidateUserSpace(string username, Guid spaceId)
    {
        var user = await _userRepository.GetByUsername(username);

        if (user is null)
        {
            throw new InvalidDataException($"User with username '{username}' not found.");
        }

        return (await _spaceRepository.GetListByUseId(user.Id)).Any(x => x.Id == spaceId);
    }
}