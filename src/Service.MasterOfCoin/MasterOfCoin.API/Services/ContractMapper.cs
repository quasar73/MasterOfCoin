using MasterOfCoin.API.ApiContracts.Auth;
using MasterOfCoin.API.Services.Interfaces;
using MasterOfCoin.API.Services.Models;

namespace MasterOfCoin.API.Services;

public class ContractMapper : IContractMapper
{
    // To Service Models
    public RegisterInfo ToRegisterInfo(RegisterRequest request)
    {
        return new(request.Username, request.Password, request.DisplayedName, request.Email);
    }

    // To Api Contracts
    public LoginResponse ToLoginResponse(LoginState state)
    {
        return new(state.Token, state.RefreshToken);
    }
}