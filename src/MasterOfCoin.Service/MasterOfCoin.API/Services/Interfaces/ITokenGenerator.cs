using MasterOfCoin.API.Data.Models;
using MasterOfCoin.API.Options;

namespace MasterOfCoin.API.Services.Interfaces;

public interface ITokenGenerator
{
    /// <summary>
    /// Generate authentication token
    /// </summary>
    /// <param name="user">User in db</param>
    /// <param name="authOptions">Authentication options</param>
    /// <returns>(token, refreshToken)</returns>
    public (string, string) GenerateToken(UserInDb user, AuthenticationOptions authOptions);
}