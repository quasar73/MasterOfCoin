using Base.Cache.Contracts;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using StackExchange.Redis.MultiplexerPool;

namespace Lib.Cache;

internal class MultiplexerPoolDatabaseProvider(
    IConnectionMultiplexerPool connectionPool,
    ILogger<MultiplexerPoolDatabaseProvider> logger)
    : IDatabaseProvider
{
    public async Task<IDatabase?> GetDatabase()
    {
        try
        {
            var connection = await connectionPool.GetAsync();
            return connection.Connection.GetDatabase();
        }
        catch (Exception e)
        {
            logger.ErrorGetConnectionFromPool(e);
            return null;
        }
    }
}