using Categories.API.Data.Models;

namespace Categories.API.Data.Interfaces;

public interface ICategoryRepository
{
    Task<int> Create(CategoryInDb categoryInDb);
    Task<int> Update(CategoryInDb categoryInDb);
    Task<int> Delete(Guid id, Guid spaceId);
    Task<CategoryInDb?> Find(Guid id, Guid spaceId);
    Task<List<CategoryInDb>> GetList(Guid spaceId);
}