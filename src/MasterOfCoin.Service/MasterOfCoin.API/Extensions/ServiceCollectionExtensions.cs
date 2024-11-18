using System.Text;
using Base.Cache.Contracts;
using MasterOfCoin.API.Data.Interfaces;
using MasterOfCoin.API.Data.Repositories;
using MasterOfCoin.API.Options;
using MasterOfCoin.API.Services;
using MasterOfCoin.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace MasterOfCoin.API.Extensions;

public static class ServiceCollectionExtensions
{
    private const string DefaultJwtKey = nameof(DefaultJwtKey);
    
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddAuthentication(configuration)
            .AddScoped<IAuthService, AuthService>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IContractMapper, ContractMapper>()
            .AddScoped<ITokenGenerator, TokenGenerator>();
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
}