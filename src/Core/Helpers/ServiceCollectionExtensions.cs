namespace Microsoft.Extensions.DependencyInjection.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ReplaceLast(this IServiceCollection services, ServiceDescriptor descriptor)
        {
            // implementation of built-in Replace is pretty inefficient: if service is already added, scans the list twice and copies rest of the list (due to usage of Remove);
            // in addition, replacing the first occurrence of the service doesn't make much sense when there are multiple registrations (the last registration is effective on resolving)
            // https://github.com/dotnet/runtime/blob/master/src/libraries/Microsoft.Extensions.DependencyInjection.Abstractions/src/Extensions/ServiceCollectionDescriptorExtensions.cs#L662

            for (int i = services.Count - 1; i >= 0; i--)
                if (services[i].ServiceType == descriptor.ServiceType)
                {
                    services[i] = descriptor;
                    return services;
                }

            services.Add(descriptor);
            return services;
        }
    }
}
