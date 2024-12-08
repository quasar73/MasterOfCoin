using MasterOfCoin.API.ApiContracts.Auth;
using MasterOfCoin.API.ApiContracts.Space;
using MasterOfCoin.API.ApiContracts.Wallet;
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
    public CreateWalletRequest ToApiCreateWalletRequest(CreateWalletApiRequest request, Guid spaceId) =>
        new(request.Name, spaceId, request.InitialValue, request.Currency, request.Cumulative);
    public EditWalletRequest ToApiEditWalletRequest(EditWalletApiRequest request, Guid spaceId) => new(
        request.WalletId,
        request.Name,
        request.Currency,
        request.Value,
        request.Cumulative,
        spaceId);
    
    // To Api Contracts
    public LoginResponse ToLoginResponse(LoginState state) => new(state.Token, state.RefreshToken);
    public SpaceResponse ToSpaceResponse(SpaceInDb space) => new(space.Id, space.Name);
    public SpaceResponse[] ToSpaceResponses(SpaceInDb[] space) => space.Select(ToSpaceResponse).ToArray();

    public CreateWalletApiResponse ToCreateWalletResponse(CreateWalletResponse response, CreateWalletRequest request) => new(
        response.WalletId!.Value,
        request.Name, 
        request.InitialValue, 
        request.Currency, 
        request.SpaceId,
        request.Cumulative);

    public WalletApiResponse ToWalletApiResponse(WalletResponse response) => new(
        response.Id,
        response.Name,
        response.Currency,
        response.Value,
        response.Cumulative);

    public GetWalletsApiResponse ToGetWalletsApiResponse(WalletsListResponse response) =>
        new(response.Wallets.Select(ToWalletApiResponse).ToArray());
}