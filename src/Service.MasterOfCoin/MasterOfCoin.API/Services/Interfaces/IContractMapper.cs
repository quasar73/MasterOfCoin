using Categories.Contracts.Contracts;
using MasterOfCoin.API.ApiContracts.Auth;
using MasterOfCoin.API.ApiContracts.Category;
using MasterOfCoin.API.ApiContracts.Space;
using MasterOfCoin.API.ApiContracts.Wallet;
using MasterOfCoin.API.Data.Models;
using MasterOfCoin.API.Services.Models;
using Wallets.Contracts.Contracts;

namespace MasterOfCoin.API.Services.Interfaces;

public interface IContractMapper
{
    // From Api Contracts
    public RegisterInfo ToRegisterInfo(RegisterRequest request);
    public CreateWalletRequest ToApiCreateWalletRequest(CreateWalletApiRequest request, Guid spaceId);
    public EditWalletRequest ToApiEditWalletRequest(EditWalletApiRequest request, Guid spaceId);
    public CreateCategoryRequest ToCreateCategoryRequest(CreateCategoryApiRequest request, Guid spaceId);
    public EditCategoryRequest ToEditCategoryRequest(EditCategoryApiRequest request, Guid spaceId);
    
    // To Api Contracts
    public LoginResponse ToLoginResponse(LoginState state);
    public SpaceResponse ToSpaceResponse(SpaceInDb space);
    public SpaceResponse[] ToSpaceResponses(SpaceInDb[] space);
    public CreateWalletApiResponse ToCreateWalletResponse(CreateWalletResponse response, CreateWalletRequest request);
    public WalletApiResponse ToWalletApiResponse(WalletResponse response);
    public GetWalletsApiResponse ToGetWalletsApiResponse(WalletsListResponse response);
    public CreateCategoryApiResponse ToCreateCategoryApiResponse(CreateCategoryResponse response, CreateCategoryRequest request);
    public CategoryApiResponse ToCategoryApiResponse(CategoryResponse response);
    public GetCategoriesApiResponse ToGetCategoriesApiResponse(CategoryResponse[] responses);
}