using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MasterOfCoin.API.Data.Models;
using MasterOfCoin.API.Options;
using MasterOfCoin.API.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace MasterOfCoin.API.Services;

public class TokenGenerator(IConfiguration _configuration) : ITokenGenerator
{
    private const int DefaultExpireTimeMinutes = 30;
    
    public string GenerateJwtToken(UserInDb user)
    {
        var authOptions = new AuthenticationOptions();
        var authSection = _configuration.GetSection(nameof(AuthenticationOptions));
        authSection.Bind(authOptions);

        var jwtKey = authOptions.JwtKey 
                     ?? throw new InvalidDataException($"{nameof(AuthenticationOptions.JwtKey)} is null");
        
        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtKey);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = GenerateClaims(user),
            Expires = DateTime.UtcNow.AddMinutes(authOptions.ExpireTimeMinutes ?? DefaultExpireTimeMinutes),
            SigningCredentials = credentials,
        };

        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
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