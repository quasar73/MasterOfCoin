using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MasterOfCoin.API.Data.Models;
using MasterOfCoin.API.Options;
using MasterOfCoin.API.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace MasterOfCoin.API.Services;

public class TokenGenerator : ITokenGenerator
{
    private const int DefaultExpireTimeMinutes = 30;
    private const int DefaultRefreshTokenLength = 64;
    
    public (string, string) GenerateToken(UserInDb user, AuthenticationOptions authOptions)
    {
        var jwtKey = authOptions.JwtKey 
                     ?? throw new InvalidDataException($"{nameof(AuthenticationOptions.JwtKey)} is null");
        
        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtKey);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature);

        var refreshToken = GenerateRefreshToken();
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = GenerateClaims(user),
            Expires = DateTime.UtcNow.AddMinutes(authOptions.ExpireTimeMinutes ?? DefaultExpireTimeMinutes),
            SigningCredentials = credentials,
        };

        var token = handler.CreateToken(tokenDescriptor);
        return (handler.WriteToken(token), refreshToken);
    }

    private string GenerateRefreshToken()
    {
        using var rng = new RNGCryptoServiceProvider();
        var tokenData = new byte[DefaultRefreshTokenLength];
        rng.GetBytes(tokenData);

        return Convert.ToBase64String(tokenData)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    private static ClaimsIdentity GenerateClaims(UserInDb user)
    {
        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim(ClaimTypes.Name, user.Username));
        claims.AddClaim(new Claim(ClaimTypes.Email, user.Email));
        claims.AddClaim(new Claim(ClaimTypes.GivenName, user.DisplayedName));

        return claims;
    }
}