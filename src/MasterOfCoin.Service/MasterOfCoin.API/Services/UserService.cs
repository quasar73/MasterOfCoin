using MasterOfCoin.API.Data.Interfaces;
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
}