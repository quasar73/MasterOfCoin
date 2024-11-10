namespace MasterOfCoin.API.Data.Models;

public record UserInDb(
    Guid Id, 
    string Username, 
    string PasswordHash, 
    string Email, 
    string? Avatar, 
    string DisplayedName);