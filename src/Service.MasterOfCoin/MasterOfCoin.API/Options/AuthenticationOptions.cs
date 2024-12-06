namespace MasterOfCoin.API.Options;

public class AuthenticationOptions
{
    public bool ValidateIssuer { get; init; }
    public bool ValidateAudience { get; init; }
    public bool ValidateLifetime { get; init; }
    public bool ValidateIssuerSigningKey { get; init; }
    public string ValidIssuer { get; init; } = default!;
    public string? JwtKey { get; init; } = default!;
    public int? ExpireTimeMinutes { get; init; } = default!;
    public int? RefreshTokenExpireTimeMinutes { get; init; } = default!;
}