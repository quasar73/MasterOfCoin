using Lib.Db.Builders;

namespace Lib.Db.Extensions;

public static class DatabaseExtensions
{
    public static DatabaseQuery Query(this IDatabase database, string table)
        => new DatabaseQuery(table, database);
}
