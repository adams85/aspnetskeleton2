using WebApp.Core.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCoreServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IGuidProvider, DefaultGuidProvider>();
        services.AddSingleton<IRandom, DefaultRandom>();
        services.AddSingleton<IClock, DefaultClock>();

        return services;
    }
}
