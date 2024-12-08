using Categories.Contracts.Contracts.Enums;

namespace Categories.Contracts.Contracts;

public record CreateCategoryResponse(RequestStatus Status, Guid? CategoryId);