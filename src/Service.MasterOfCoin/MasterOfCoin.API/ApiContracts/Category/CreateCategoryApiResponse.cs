namespace MasterOfCoin.API.ApiContracts.Category;

public record CreateCategoryApiResponse(
    Guid Id,
    string Name, 
    Guid? ParentId, 
    string? Color, 
    string? Icon);