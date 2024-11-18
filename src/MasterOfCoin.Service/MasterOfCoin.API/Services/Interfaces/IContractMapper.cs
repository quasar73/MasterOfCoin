using MasterOfCoin.API.ApiContracts.Auth;
using MasterOfCoin.API.Services.Models;

namespace MasterOfCoin.API.Services.Interfaces;

public interface IContractMapper
{
    // From Api Contracts
    public RegisterInfo ToRegisterInfo(RegisterRequest request);
    
    // To Api Contracts
    public LoginResponse ToLoginResponse(LoginState state);
}