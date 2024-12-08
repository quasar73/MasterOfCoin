namespace Categories.Contracts.Contracts;

public record CategoryResponse(
    Guid Id,
    string Name,
    Guid? ParentId,
    string? Color,
    string? Icon,
    Guid SpaceId);