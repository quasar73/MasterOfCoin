using MasterOfCoin.API.Data.Models;

namespace MasterOfCoin.API.Services.Interfaces;

public interface ITokenGenerator
{
    public string GenerateJwtToken(UserInDb user);
}