using System.Text;
using Base.Cache.Contracts;
using Lib.CrossService.Models;
using MasterOfCoin.API.Data.Interfaces;
using MasterOfCoin.API.Data.Repositories;
using MasterOfCoin.API.Options;
using MasterOfCoin.API.Services;
using MasterOfCoin.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Transactions.Contracts.Interfaces;
using Lib.CrossService.Extensions;
using Wallets.Contracts.Interfaces;

namespace MasterOfCoin.API.Extensions;

public static class ServiceCollectionExtensions
{
    private const string DefaultJwtKey = nameof(DefaultJwtKey);
    
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration, bool isLocal)
    {
        return services
            .AddAuthentication(configuration)
            .AddGrpcClients(isLocal)
            .AddRepositories()
            .AddCustomServices();
    }

    private static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<IAuthService, AuthService>()
            .AddSingleton<IContractMapper, ContractMapper>()
            .AddSingleton<ITokenGenerator, TokenGenerator>()
            .AddSingleton<IValidationService, ValidationService>()
            .AddSingleton<ISpaceService, SpaceService>();
    }
    
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddSingleton<IUserRepository, UserRepository>()
            .AddSingleton<ISpaceRepository, SpaceRepository>();
    }

    private static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authOptions = new AuthenticationOptions();
        var authSection = configuration.GetSection(nameof(AuthenticationOptions));
        authSection.Bind(authOptions);
        
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var cache = context.HttpContext.RequestServices.GetRequiredService<ICacheStore>();
                        
                        if (!context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
                        {
                            return;
                        }
        
                        var token = authorizationHeader.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
                        
                        if (!(string.IsNullOrEmpty(token) || string.IsNullOrEmpty(await cache.GetAsync(token.ToInvalidTokenKey()))))
                        {
                            context.Fail("Token is invalid.");
                            Console.WriteLine(token);
                        }
                    }
                };
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = authOptions.ValidateIssuer,
                    ValidateAudience = authOptions.ValidateAudience,
                    ValidateLifetime = authOptions.ValidateLifetime,
                    ValidateIssuerSigningKey = authOptions.ValidateIssuerSigningKey,
                    ValidIssuer = authOptions.ValidIssuer,
                    ValidAudience = authOptions.ValidIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.JwtKey ?? DefaultJwtKey))
                };
            });

        return services;
    }
    
    private static IServiceCollection AddGrpcClients(this IServiceCollection services, bool isLocal)
    {
        _ = bool.TryParse(Environment.GetEnvironmentVariable("ServerConnectionSettings__UseLoadBalancing"), out var useLoadBalancing);
        var grpcSettings = new ServerConnectionSettings { MapToGatewayRoot = isLocal, UseLoadBalancing = useLoadBalancing };

        if (isLocal)
        {
            var transactionsGateway = Environment.GetEnvironmentVariable("LocalGrpc__TransactionsGatewayUri")!;
            var walletsGateway = Environment.GetEnvironmentVariable("LocalGrpc__WalletsGatewayUri")!;

            services.AddGrpcClients(transactionsGateway, grpcSettings, typeof(ITransactionsApi).Assembly);
            services.AddGrpcClients(walletsGateway, grpcSettings, typeof(IWalletsApi).Assembly);
        }
        else
        {
            services.AddGrpcClients(Environment.GetEnvironmentVariable("ConnectionStrings__GrpcGatewayUri")!, grpcSettings, typeof(ITransactionsApi).Assembly);
            services.AddGrpcClients(Environment.GetEnvironmentVariable("ConnectionStrings__GrpcGatewayUri")!, grpcSettings, typeof(IWalletsApi).Assembly);
        }

        return services;
    }
}