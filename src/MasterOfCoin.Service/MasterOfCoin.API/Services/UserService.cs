using MasterOfCoin.API.Data.Interfaces;
using MasterOfCoin.API.Data.Models;
using MasterOfCoin.API.Extensions;
using MasterOfCoin.API.Services.Interfaces;
using MasterOfCoin.API.Services.Models;

namespace MasterOfCoin.API.Services;

public class UserService(ITokenGenerator _tokenGenerator, IUserRepository _userRepository) : IUserService
{
    public async Task<LoginState> Authorize(string username, string password)
    {
        var userInDb = await _userRepository.AuthorizeInDb(username, password);

        if (userInDb is null)
        {
            return new(LoginStatus.Unauthorized, string.Empty);
        }

        return new(LoginStatus.Success, _tokenGenerator.GenerateJwtToken(userInDb));
    }

    public async Task<RegisterStatus> Register(RegisterInfo info)
    {
        var salt = Guid.NewGuid().ToByteArray();
        var hash = info.Password.CalculatePasswordHash(salt);
        var userInDb = new UserInDb(
            Guid.NewGuid(),
            info.Username,
            hash,
            salt,
            info.Email,
            null,
            info.DisplayedName);
        
        try
        {
            await _userRepository.Create(userInDb);
        }
        catch
        {
            return RegisterStatus.Unregister;
        }

        return RegisterStatus.Success;
    }
}