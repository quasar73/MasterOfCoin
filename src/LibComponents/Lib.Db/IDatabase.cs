namespace Lib.Db;

public interface IDatabase : IDatabaseBase
{
    /// <summary>
    /// Does some actions in transaction
    /// </summary>
    Task DoInTransaction(Func<IDatabaseTransaction, Task> action);

    /// <summary>
    /// Does some actions in transaction and returns a result of operation
    /// </summary>
    Task<T?> DoInTransaction<T>(Func<IDatabaseTransaction, Task<T?>> action);
}
