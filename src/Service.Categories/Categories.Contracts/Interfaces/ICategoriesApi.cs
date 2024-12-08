using Categories.Contracts.Contracts;

namespace Categories.Contracts.Interfaces;

public interface ICategoriesApi
{
    Task<CreateCategoryResponse> CreateCategory(CreateCategoryRequest request);
    Task<StatusResponse> EditCategory(EditCategoryRequest request);
    Task<CategoriesListResponse> GetCategories(GetCategoriesRequest request);
    Task<GetCategoryResponse> GetCategory(GetCategoryRequest request);
}