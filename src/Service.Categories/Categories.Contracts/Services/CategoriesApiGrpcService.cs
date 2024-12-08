using Castle.DynamicProxy;
using Categories.Contracts.Interfaces;
using Categories.Contracts.Protobuf;
using Lib.CrossService.Interfaces;

namespace Categories.Contracts.Services;

public class CategoriesApiGrpcService : CategoriesApi.CategoriesApiBase, IGrpcService<ICategoriesApi>
{
    private CategoriesApiGrpcService() { }
    
    public CategoriesApiGrpcService(IInterceptorSelector _) { }
}