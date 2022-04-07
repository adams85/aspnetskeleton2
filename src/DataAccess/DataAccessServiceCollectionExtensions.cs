using System;
using Microsoft.Extensions.Options;
using WebApp.DataAccess;

namespace Microsoft.Extensions.DependencyInjection;

public static class DataAccessLayerServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IServiceProvider optionsProvider)
    {
        var dataAccessOptions = optionsProvider.GetRequiredService<IOptions<DataAccessOptions>>().Value;

        services.AddCoreServices();

        EFCoreConfiguration.From(dataAccessOptions)
            .ConfigureServices<ReadOnlyDataContext>(services)
            .ConfigureServices<WritableDataContext>(services);

        return services;
    }
}
