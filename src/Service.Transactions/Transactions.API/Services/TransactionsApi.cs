using Transactions.Contracts.Contracts.Transactions;
using Transactions.Contracts.Interfaces;

namespace Transactions.API.Services;

public class TransactionsApi : ITransactionsApi
{
    public async Task<TestResponse> TestMethod(TestRequest request)
    {
        return new($"Message with value: {request.Value}");
    }
}