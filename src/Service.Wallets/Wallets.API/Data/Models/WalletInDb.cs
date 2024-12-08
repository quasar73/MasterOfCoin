namespace Wallets.API.Data.Models;

public class WalletInDb
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Value { get; set; }
    public string Currency { get; set; } = default!;
    public bool Cumulative { get; set; }
    public Guid SpaceId { get; set; }
    public Guid AccountId { get; set; }
    public bool Archived { get; set; }
}