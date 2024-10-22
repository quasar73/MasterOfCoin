using Npgsql;

namespace Lib.Db;

public interface IDatabaseFactory
{
    IDatabase Create(string connectionString);
}
