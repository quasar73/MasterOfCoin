using Categories.Contracts.Interfaces;
using Categories.Contracts.Protobuf;
using Grpc.Core;
using Lib.CrossService.Interfaces;

namespace Categories.Contracts.Clients;

public class CategoriesApiGrpcClient(CallInvoker invoker) : CategoriesApi.CategoriesApiClient(invoker), IGrpcClient<ICategoriesApi>
{
}