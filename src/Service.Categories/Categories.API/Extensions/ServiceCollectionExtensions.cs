using Categories.API.Data.Interfaces;
using Categories.API.Data.Repositories;
using Categories.API.Services;
using Categories.API.Services.Interfaces;
using Categories.Contracts.Interfaces;

namespace Categories.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<IModelMapper, ModelMapper>()
            .AddRepositories()
            .AddApi();
    }
    
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddSingleton<ICategoryRepository, CategoryRepository>();
    }
    
    private static IServiceCollection AddApi(this IServiceCollection services)
    {
        return services
            .AddSingleton<ICategoriesApi, CategoriesApi>();
    }
}