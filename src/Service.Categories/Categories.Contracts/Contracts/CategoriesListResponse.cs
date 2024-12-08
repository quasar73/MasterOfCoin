using Categories.Contracts.Contracts.Enums;

namespace Categories.Contracts.Contracts;

public record CategoriesListResponse(RequestStatus Status, CategoryResponse[] Categories);