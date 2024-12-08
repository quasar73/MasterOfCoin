using Categories.Contracts.Contracts.Enums;

namespace Categories.Contracts.Contracts;

public record GetCategoryResponse(RequestStatus Status, CategoryResponse? Category);