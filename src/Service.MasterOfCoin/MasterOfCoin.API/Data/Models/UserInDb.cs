namespace MasterOfCoin.API.Data.Models;

public class UserInDb
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public byte[] PasswordSalt { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Avatar { get; set; }
    public string DisplayedName { get; set; } = default!;
}