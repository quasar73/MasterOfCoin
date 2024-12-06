namespace MasterOfCoin.API.Data.Models;

public class SpaceInDb
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public Guid UserId { get; set; }
    public bool Deleted { get; set; }
}