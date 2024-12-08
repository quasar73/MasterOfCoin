using Categories.API.Data.Interfaces;
using Categories.API.Data.Models;
using Lib.Db;

namespace Categories.API.Data.Repositories;

public class CategoryRepository(IDatabase _database) : ICategoryRepository
{
    public Task<int> Create(CategoryInDb categoryInDb) => _database.Execute(
        "INSERT INTO categories(id, name, space_id, parent_id, icon, color, account_id)" +
        "VALUES (@Id, @Name, @SpaceId, @ParentId, @Icon, @Color, @AccountId)", categoryInDb);

    public Task<int> Update(CategoryInDb categoryInDb) => _database.Execute(
        "UPDATE categories SET name = @Name, parent_id = @ParentId, icon = @Icon, color = @Color WHERE id = @Id AND space_id = @SpaceId",
        categoryInDb);

    public Task<int> Delete(Guid id, Guid spaceId) => _database.Execute(
        "DELETE FROM categories WHERE id = @id AND space_id = @spaceId", new { id, spaceId });

    public Task<CategoryInDb?> Find(Guid id, Guid spaceId) => _database.GetOrDefault<CategoryInDb>(
        "SELECT * FROM categories WHERE id = @id AND space_id = @spaceId", new { id, spaceId });

    public Task<List<CategoryInDb>> GetList(Guid spaceId) => _database.GetList<CategoryInDb>(
        "SELECT * FROM categories WHERE space_id = @spaceId", new { spaceId });
}