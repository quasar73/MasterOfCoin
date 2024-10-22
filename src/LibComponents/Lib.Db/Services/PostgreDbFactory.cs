namespace Lib.Db.Services;

public class PostgreDbFactory(IDataSourceHolder dataSourceHolder) : IDatabaseFactory
{
    public IDatabase Create(string connectionString) => new PostgreDb(dataSourceHolder.GetDataSource(connectionString));
}
