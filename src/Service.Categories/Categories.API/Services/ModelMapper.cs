using Categories.API.Data.Models;
using Categories.API.Services.Interfaces;
using Categories.Contracts.Contracts;
using Transactions.Contracts.Messages.Accounts;
using Transactions.Contracts.Messages.Accounts.Enums;

namespace Categories.API.Services;

public class ModelMapper : IModelMapper
{
    public CategoryInDb ToCategoryInDb(CreateCategoryRequest request) => new CategoryInDb
    {
        Id = Guid.NewGuid(),
        Name = request.Name,
        ParentId = request.ParentId,
        Icon = request.Icon,
        Color = request.Color,
        SpaceId = request.SpaceId,
        AccountId = Guid.NewGuid()
    };

    public CategoryResponse[] ToCategoryResponses(List<CategoryInDb> categories) =>
        categories.Select(ToCategoryResponse).ToArray();

    public CategoryResponse ToCategoryResponse(CategoryInDb categoryInDb) => new(
        categoryInDb.Id,
        categoryInDb.Name,
        categoryInDb.ParentId,
        categoryInDb.Color,
        categoryInDb.Icon,
        categoryInDb.SpaceId);

    public CreateAccountMessage ToCreateAccountMessage(CategoryInDb categoryInDb) => new(
        categoryInDb.AccountId,
        categoryInDb.SpaceId,
        AccountCreatingSource.Category);
}