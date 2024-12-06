using Castle.DynamicProxy;
using Lib.CrossService.Interfaces;
using Transactions.Contracts.Interfaces;
using Transactions.Contracts.Protobuf;

namespace Transactions.Contracts.Services;

public class TransactionsApiGrpcService : TransactionsApi.TransactionsApiBase, IGrpcService<ITransactionsApi>
{
    private TransactionsApiGrpcService() { }
    
    public TransactionsApiGrpcService(IInterceptorSelector _) { }
}