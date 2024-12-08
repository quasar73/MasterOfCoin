namespace MasterOfCoin.API.ApiContracts.Category;

public record EditCategoryApiRequest(
    Guid Id,
    string? Name,
    Guid? ParentId,
    string? Color,
    string? Icon);