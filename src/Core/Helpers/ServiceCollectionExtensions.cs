namespace Microsoft.Extensions.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ReplaceLast(this IServiceCollection services, ServiceDescriptor descriptor) =>
        services.ReplaceLast(descriptor, out var _);

    public static IServiceCollection ReplaceLast(this IServiceCollection services, ServiceDescriptor descriptor, out ServiceDescriptor? replacedDescriptor)
    {
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
