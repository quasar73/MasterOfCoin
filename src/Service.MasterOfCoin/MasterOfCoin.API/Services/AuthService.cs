using Base.Cache.Contracts;
using MasterOfCoin.API.Data.Interfaces;
using MasterOfCoin.API.Data.Models;
using MasterOfCoin.API.Extensions;
using MasterOfCoin.API.Options;
using MasterOfCoin.API.Services.Interfaces;
using MasterOfCoin.API.Services.Models;

namespace MasterOfCoin.API.Services;

public class AuthService : IAuthService
{
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IUserRepository _userRepository;
    private readonly ICacheStore _cacheStore;
    private AuthenticationOptions _authOptions = default!;
    
    private const int DefaultExpireTime = 60;
    private const int RefreshTokenDefaultExpireTime = 360;
    
    public AuthService(
        ITokenGenerator tokenGenerator, 
        IUserRepository userRepository, 
        ICacheStore cacheStore, 
        IConfiguration configuration)
    {
        _tokenGenerator = tokenGenerator;
        _userRepository = userRepository;
        _cacheStore = cacheStore;
        
        _authOptions = configuration.GetSection(nameof(AuthenticationOptions)).Get<AuthenticationOptions>() ?? new();
    }
    
    public async Task<LoginState> Authorize(string username, string password)
    {
        var userInDb = await _userRepository.AuthorizeInDb(username, password);

        if (userInDb is null)
        {
            return new(LoginStatus.Unauthorized, string.Empty, string.Empty);
        }

        return await Authorize(userInDb);
    }

    public async Task<LoginState> Refresh(string refreshToken)
    {
        var userIdInCache = (await _cacheStore.GetAsync(refreshToken.ToRefreshTokenKey()))?.Trim('"');

        return await Authorize(userIdInCache);
    }

    public async Task InvalidateToken(string token)
    {
        await _cacheStore.SetAsync(token.ToInvalidTokenKey(), true, TimeSpan.FromMinutes(_authOptions.ExpireTimeMinutes ?? DefaultExpireTime));
    }

    public async Task<RegisterStatus> Register(RegisterInfo info)
    {
        var salt = Guid.NewGuid().ToByteArray();
        var hash = info.Password.CalculatePasswordHash(salt);
        var userInDb = new UserInDb
        {
            Id = Guid.NewGuid(),
            Username = info.Username,
            PasswordHash = hash,
            PasswordSalt = salt,
            Email = info.Email,
            Avatar = null,
            DisplayedName = info.DisplayedName
        };
        
        try
        {
            await _userRepository.Create(userInDb);
        }
        catch
        {
            return RegisterStatus.Unregister;
        }

        return RegisterStatus.Success;
    }

    private async Task<LoginState> Authorize(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidDataException("User Id is null");
        }

        var userInDb = await _userRepository.GetById(Guid.Parse(userId));
        
        if (userInDb is null)
        {
            return new(LoginStatus.Unauthorized, string.Empty, string.Empty);
        }

        return await Authorize(userInDb);
    }

    private async Task<LoginState> Authorize(UserInDb userInDb)
    {
        var (token, refreshToken) = _tokenGenerator.GenerateToken(userInDb, _authOptions);

        await _cacheStore.SetAsync(refreshToken.ToRefreshTokenKey(), userInDb.Id,
            TimeSpan.FromMinutes(_authOptions.RefreshTokenExpireTimeMinutes ?? RefreshTokenDefaultExpireTime));
        
        return new(LoginStatus.Success, token, refreshToken);
    }
}