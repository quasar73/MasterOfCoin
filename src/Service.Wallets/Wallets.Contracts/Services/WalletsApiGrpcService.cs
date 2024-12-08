using Castle.DynamicProxy;
using Lib.CrossService.Interfaces;
using Wallets.Contracts.Interfaces;
using Wallets.Contracts.Protobuf;

namespace Wallets.Contracts.Services;

public class WalletsApiGrpcService : WalletsApi.WalletsApiBase, IGrpcService<IWalletsApi>
{
    private WalletsApiGrpcService() { }
    
    public WalletsApiGrpcService(IInterceptorSelector _) { }
}