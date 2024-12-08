namespace Categories.Contracts.Contracts;

public record EditCategoryRequest(
    Guid Id,
    string? Name,
    Guid? ParentId,
    string? Color,
    string? Icon,
    Guid SpaceId);