using Grpc.Core;
using Lib.CrossService.Interfaces;
using Wallets.Contracts.Interfaces;
using Wallets.Contracts.Protobuf;

namespace Wallets.Contracts.Clients;

public class WalletsApiGrpcClient(CallInvoker invoker) : WalletsApi.WalletsApiClient(invoker), IGrpcClient<IWalletsApi>
{
}