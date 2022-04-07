using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp.UI.Infrastructure.Hosting;

public class MultitenantServiceProviderFactory : AutofacServiceProviderFactory, IServiceProviderFactory<ContainerBuilder>
{
    ContainerBuilder IServiceProviderFactory<ContainerBuilder>.CreateBuilder(IServiceCollection services) => CreateBuilder(services);

    IServiceProvider IServiceProviderFactory<ContainerBuilder>.CreateServiceProvider(ContainerBuilder containerBuilder)
    {
        var rootServiceProvider = (AutofacServiceProvider)CreateServiceProvider(containerBuilder);

        var tenants = rootServiceProvider.GetRequiredService<Tenants>();
        tenants.InitializeServices(rootServiceProvider);

        return rootServiceProvider;
    }
}
