using Lib.Db.Models;
using Dapper;
using System.Data;

namespace Lib.Db.Utils;

public class DbEnumHandler<TEnum> : SqlMapper.ITypeHandler where TEnum : Enum
{
    public object Parse(Type destinationType, object value)
    {
        if (destinationType == typeof(DbEnum<TEnum>))
        {
            return new DbEnum<TEnum>((string)value);
        }

        throw new InvalidCastException($"Can't parse string value {value} into enum type {typeof(TEnum).Name}");
    }

    public void SetValue(IDbDataParameter parameter, object value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = ((DbEnum<TEnum>)value).ToString();
    }
}
