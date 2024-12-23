﻿using Transactions.API.Data.Interfaces;
using Transactions.API.Data.Repositories;
using Transactions.API.Services;
using Transactions.Contracts.Interfaces;

namespace Transactions.API.Extensions;

public static class ServiceCollectionExtensions
{
    private const string DefaultJwtKey = nameof(DefaultJwtKey);
    
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services
            .AddRepositories()
            .AddApi();
    }
    
    private static IServiceCollection AddApi(this IServiceCollection services)
    {
        return services
            .AddSingleton<ITransactionsApi, TransactionsApi>();
    }
    
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddSingleton<IAccountRepository, AccountRepository>();
    }
}