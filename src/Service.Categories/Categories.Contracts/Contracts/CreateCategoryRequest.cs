namespace Categories.Contracts.Contracts;

public record CreateCategoryRequest(
    string Name,
    Guid? ParentId,
    string? Color,
    string? Icon,
    Guid SpaceId);