namespace Microsoft.Extensions.DependencyInjection.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ReplaceLast(this IServiceCollection services, ServiceDescriptor descriptor) =>
            services.ReplaceLast(descriptor, out var _);

        public static IServiceCollection ReplaceLast(this IServiceCollection services, ServiceDescriptor descriptor, out ServiceDescriptor? replacedDescriptor)
        {
            // implementation of built-in Replace is pretty inefficient: if service is already added, scans the list twice and copies rest of the list (due to usage of Remove);
            // in addition, replacing the first occurrence of the service doesn't make much sense when there are multiple registrations (the last registration is effective on resolving)
            // https://github.com/dotnet/extensions/blob/v3.1.18/src/DependencyInjection/DI.Abstractions/src/Extensions/ServiceCollectionDescriptorExtensions.cs#L663

            for (int i = services.Count - 1; i >= 0; i--)
            {
                var currentDescriptor = services[i];

                if (currentDescriptor.ServiceType == descriptor.ServiceType)
                {
                    services[i] = descriptor;

                    replacedDescriptor = currentDescriptor;
                    return services;
                }
            }

            services.Add(descriptor);

            replacedDescriptor = null;
            return services;
        }
    }
}
