using MasterOfCoin.API.ApiContracts.Auth;
using MasterOfCoin.API.ApiContracts.Space;
using MasterOfCoin.API.Data.Models;
using MasterOfCoin.API.Services.Interfaces;
using MasterOfCoin.API.Services.Models;

namespace MasterOfCoin.API.Services;

public class ContractMapper : IContractMapper
{
    // To Service Models
    public RegisterInfo ToRegisterInfo(RegisterRequest request) =>
        new(request.Username, request.Password, request.DisplayedName, request.Email);

    // To Api Contracts
    public LoginResponse ToLoginResponse(LoginState state) => new(state.Token, state.RefreshToken);

    public SpaceResponse ToSpaceResponse(SpaceInDb space) => new(space.Id, space.Name);
    public SpaceResponse[] ToSpaceResponses(SpaceInDb[] space) => space.Select(ToSpaceResponse).ToArray();
}