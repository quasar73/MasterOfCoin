using Categories.API.Data.Models;
using Categories.Contracts.Contracts;
using Transactions.Contracts.Messages.Accounts;

namespace Categories.API.Services.Interfaces;

public interface IModelMapper
{
    CategoryInDb ToCategoryInDb(CreateCategoryRequest request);
    CategoryResponse[] ToCategoryResponses(List<CategoryInDb> categories);
    CategoryResponse ToCategoryResponse(CategoryInDb categoryInDb);
    CreateAccountMessage ToCreateAccountMessage(CategoryInDb categoryInDb);
}