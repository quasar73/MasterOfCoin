using Wallets.API.Data.Interfaces;
using Wallets.API.Data.Repositories;
using Wallets.API.Services;
using Wallets.API.Services.Interfaces;
using Wallets.Contracts.Interfaces;

namespace Wallets.API.Extensions;

public static class ServiceCollectionExtensions
{
    private const string DefaultJwtKey = nameof(DefaultJwtKey);
    
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services
            .AddRepositories()
            .AddApi()
            .AddSingleton<IModelMapper, ModelMapper>();
    }
    
    private static IServiceCollection AddApi(this IServiceCollection services)
    {
        return services
            .AddSingleton<IWalletsApi, WalletsApi>();
    }
    
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddSingleton<IWalletRepository, WalletRepository>();
    }
}