using MasterOfCoin.API.ApiContracts.Auth;
using MasterOfCoin.API.ApiContracts.Space;
using MasterOfCoin.API.Data.Models;
using MasterOfCoin.API.Services.Interfaces;
using MasterOfCoin.API.Services.Models;
using Wallets.Contracts.Contracts;

namespace MasterOfCoin.API.Services;

public class ContractMapper : IContractMapper
{
    // To Service Models
    public RegisterInfo ToRegisterInfo(RegisterRequest request) =>
        new(request.Username, request.Password, request.DisplayedName, request.Email);
    public CreateWalletRequest ToApiCreateWalletRequest(ApiContracts.Wallet.CreateWalletRequest request) =>
        new(request.Name, request.SpaceId, request.InitialValue, request.Currency, request.Cumulative);
    
    // To Api Contracts
    public LoginResponse ToLoginResponse(LoginState state) => new(state.Token, state.RefreshToken);
    public SpaceResponse ToSpaceResponse(SpaceInDb space) => new(space.Id, space.Name);
    public SpaceResponse[] ToSpaceResponses(SpaceInDb[] space) => space.Select(ToSpaceResponse).ToArray();

    public ApiContracts.Wallet.CreateWalletResponse ToCreateWalletResponse(
        CreateWalletResponse response, CreateWalletRequest request) => new(
        response.WalletId,
        request.Name, 
        request.InitialValue, 
        request.Currency, 
        request.SpaceId,
        request.Cumulative);
}