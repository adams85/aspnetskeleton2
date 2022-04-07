namespace WebApp.Service.Infrastructure;

internal interface IServiceHostGrpcServiceFactory
{
    TService CreateGrpcService<TService>() where TService : class;
}
