using Microsoft.AspNetCore.Cryptography.KeyDerivation;

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
}