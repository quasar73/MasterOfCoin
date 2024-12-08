namespace MasterOfCoin.API.ApiContracts.Category;

public record CreateCategoryApiRequest(string Name, Guid? ParentId, string? Color, string? Icon);