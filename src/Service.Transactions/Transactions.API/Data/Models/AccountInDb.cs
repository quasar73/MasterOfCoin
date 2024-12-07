namespace Transactions.API.Data.Models;

public class AccountInDb
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public Guid SpaceId { get; set; }
}