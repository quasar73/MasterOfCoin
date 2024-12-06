using Transactions.API.Services;
using Transactions.Contracts.Interfaces;

namespace Transactions.API.Extensions;

public static class ServiceCollectionExtensions
{
    private const string DefaultJwtKey = nameof(DefaultJwtKey);
    
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services
            .AddApi();
    }
    
    private static IServiceCollection AddApi(this IServiceCollection services)
    {
        return services
            .AddSingleton<ITransactionsApi, TransactionsApi>();
    }
}