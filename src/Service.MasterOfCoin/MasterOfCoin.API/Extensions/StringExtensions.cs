using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using static MasterOfCoin.API.Constants;

namespace MasterOfCoin.API.Extensions;

public static class StringExtensions
{
    public static string CalculatePasswordHash(this string password, byte[] salt)
    {
        var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password!,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
        return hashed;
    }

    public static string ToInvalidTokenKey(this string token)
    {
        return $"{CachePrefixes.InvalidToken}{token}";
    }
    
    public static string ToRefreshTokenKey(this string token)
    {
        return $"{CachePrefixes.RefreshToken}{token}";
    }
}