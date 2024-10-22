using Npgsql;
using System.Collections.Concurrent;

namespace Lib.Db.Services;

public class DataSourceHolder : IDataSourceHolder
{
    private readonly ConcurrentDictionary<string, NpgsqlDataSource> _dataSources = new();

    public NpgsqlDataSource GetDataSource(string connectionString) =>
        _dataSources.AddOrUpdate(connectionString, cs => new NpgsqlDataSourceBuilder(cs).Build(), (_, old) => old);
}
