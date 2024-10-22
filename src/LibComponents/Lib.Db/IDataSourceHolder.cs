using Npgsql;

namespace Lib.Db;

public interface IDataSourceHolder
{
    NpgsqlDataSource GetDataSource(string connectionString);
}
