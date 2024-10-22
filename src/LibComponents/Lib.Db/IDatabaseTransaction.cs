namespace Lib.Db;

public interface IDatabaseTransaction : IDatabaseBase, IAsyncDisposable
{
    Task Rollback();
}
