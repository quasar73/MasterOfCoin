namespace Lib.Db.Builders;

public partial class BaseQuery<TQuery> where TQuery : BaseQuery<TQuery>
{
    public TQuery WhereTrue(string column)
    {
        _whereClauses.Add(column);
        return (TQuery)this;
    }

    public TQuery WhereFalse(string column)
        => WhereTrue($"NOT {column}");

    public TQuery Where(string column, string value)
        => WhereTrue($"{column} = {value}");

    /// <summary>
    /// Add where statement if condition is true
    /// </summary>
    public TQuery Where(string column, string parameter, Func<bool> condition)
    {
        return condition()
            ? Where(column, parameter)
            : (TQuery)this;
    }

    public TQuery WhereNull(string column)
        => WhereTrue($"{column} IS NULL");

    public TQuery WhereNotNull(string column)
        => WhereTrue($"{column} IS NOT NULL");

    public TQuery WhereAny(string column, string value)
        => WhereTrue($"{column} = ANY({value})");

    public TQuery WhereMore(string column, string value)
        => WhereTrue($"{column} > {value}");

    public TQuery WhereLess(string column, string value)
        => WhereTrue($"{column} < {value}");

    public TQuery WhereBetween(string column, string from, string to)
        => WhereTrue($"{column} BETWEEN {from} AND {to}");
}
