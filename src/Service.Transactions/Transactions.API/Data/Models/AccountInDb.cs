using Transactions.API.Data.Models.Enums;

namespace Transactions.API.Data.Models;

public class AccountInDb
{
    public Guid Id { get; set; }
    public Guid SpaceId { get; set; }
    public AccountType Type { get; set; }
}