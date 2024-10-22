namespace Lib.Db.Builders;

public partial class BaseQuery<TQuery> where TQuery : BaseQuery<TQuery>
{
    public TQuery Select(params string[] columns)
    {
        foreach (var column in columns)
        {
            _selectColumns.Add(column);
        }
        return (TQuery)this;
    }

    public TQuery SelectSum(string column)
        => Select($"SUM({column})");

    public TQuery SelectCount()
        => SelectCount("*");

    public TQuery SelectCount(string column)
        => Select($"COUNT({column})");

    public TQuery SelectAvg(string column)
        => Select($"AVG({column})");

    public TQuery SelectMin(string column)
        => Select($"MIN({column})");

    public TQuery SelectMax(string column)
        => Select($"MAX({column})");
}
