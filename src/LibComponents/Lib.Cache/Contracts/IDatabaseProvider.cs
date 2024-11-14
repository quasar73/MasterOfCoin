using StackExchange.Redis;

namespace Base.Cache.Contracts;

public interface IDatabaseProvider
{
    Task<IDatabase?> GetDatabase();
}