namespace Categories.API.Data.Models;

public class CategoryInDb
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public Guid AccountId { get; set; }
    public Guid? ParentId { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public Guid SpaceId { get; set; }
}