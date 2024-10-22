using Dapper;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace Lib.Db.Models;

public class Json<T> : SqlMapper.ICustomQueryParameter
{
    public Json() { }

    public Json(T? value)
    {
        Value = value;
    }

    public Json(string value)
    {
        // case insensitive by default
        Value = JsonConvert.DeserializeObject<T>(value);
    }

    public T? Value { get; set; }

    public void AddParameter(IDbCommand command, string name)
    {
        var param = (NpgsqlParameter)command.CreateParameter();
        param.ParameterName = name;
        param.Value = JsonConvert.SerializeObject(Value);
        param.NpgsqlDbType = NpgsqlDbType.Jsonb;

        command.Parameters.Add(param);
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(Value);
    }

    public static implicit operator Json<T>(T value) => new(value);
    public static implicit operator Json<T>(string value) => new(value);
    public static implicit operator T?(Json<T> json) => json.Value;
}
