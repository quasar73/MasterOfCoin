using Grpc.Core;
using Lib.CrossService.Interfaces;
using Transactions.Contracts.Interfaces;
using Transactions.Contracts.Protobuf;

namespace Transactions.Contracts.Clients;

public class TransactionsApiGrpcClient(CallInvoker invoker) : TransactionsApi.TransactionsApiClient(invoker), IGrpcClient<ITransactionsApi>
{
}