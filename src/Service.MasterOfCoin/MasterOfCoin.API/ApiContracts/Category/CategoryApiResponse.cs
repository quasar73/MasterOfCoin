namespace MasterOfCoin.API.ApiContracts.Category;

public record CategoryApiResponse(
    Guid Id,
    string Name,
    Guid? ParentId,
    string? Color,
    string? Icon,
    Guid SpaceId);