using Lib.Db.Models;

namespace Lib.Db.Builders;

public class DatabaseQuery(string table, IDatabase database) : BaseQuery<DatabaseQuery>(table)
{
    public Task<List<T>> GetList<T>(object? parameters = null)
        => database.GetList<T>(Build(), parameters);

    public Task<T?> GetOrDefault<T>(object? parameters = null)
        => database.GetOrDefault<T>(Build(), parameters);

    public Task<int> Execute(object? parameters = null)
        => database.Execute(Build(), parameters);
}