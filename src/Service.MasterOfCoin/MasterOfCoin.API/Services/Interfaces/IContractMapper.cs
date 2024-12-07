using MasterOfCoin.API.ApiContracts.Auth;
using MasterOfCoin.API.ApiContracts.Space;
using MasterOfCoin.API.Data.Models;
using MasterOfCoin.API.Services.Models;
using Wallets.Contracts.Contracts;

namespace MasterOfCoin.API.Services.Interfaces;

public interface IContractMapper
{
    // From Api Contracts
    public RegisterInfo ToRegisterInfo(RegisterRequest request);
    public CreateWalletRequest ToApiCreateWalletRequest(ApiContracts.Wallet.CreateWalletRequest request);
    
    // To Api Contracts
    public LoginResponse ToLoginResponse(LoginState state);
    public SpaceResponse ToSpaceResponse(SpaceInDb space);
    public SpaceResponse[] ToSpaceResponses(SpaceInDb[] space);
    public ApiContracts.Wallet.CreateWalletResponse ToCreateWalletResponse(
        CreateWalletResponse response, CreateWalletRequest request);
}