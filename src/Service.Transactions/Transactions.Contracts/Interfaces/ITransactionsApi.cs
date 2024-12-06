using Transactions.Contracts.Contracts.Transactions;

namespace Transactions.Contracts.Interfaces;

public interface ITransactionsApi
{
    Task<TestResponse> TestMethod(TestRequest request);
}