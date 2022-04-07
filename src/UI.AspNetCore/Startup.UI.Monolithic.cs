using Microsoft.Extensions.DependencyInjection;
using WebApp.Service.Infrastructure.Templating;
using WebApp.UI.Infrastructure.Hosting;

namespace WebApp.UI;

public partial class Startup
{
    partial void ConfigureServicesPartial(IServiceCollection services)
    {
        services.AddTransient(sp =>
            // HACK: ITemplateRenderer must be visible for singleton services (like IMailSenderService) residing in the root container
            // but unfortunately, we cannot register it in the root container. If we did so, we would also need to add the Razor view engine services
            // in the root container which would bring along routing services. We cannot allow this as it would completely mess up routing.
            // So we register ITemplateRenderer in the UI tenant container (being an MVC app, it adds a Razor view engine anyway)
            // and add this fake registration which delegates the service resolution to the UI tenant container.
            // This is an ugly trick but we cannot do much better. It's not something hazardous though since ITemplateRenderer is a transient service and
            // the root container and tenant containers have the same lifetime.
            sp.GetRequiredService<Tenants>()[UITenantId]!.TenantServices!.GetRequiredService<ITemplateRenderer>());
    }

    private partial class UITenant
    {
        partial void ConfigureMvcPartial(IMvcBuilder builder)
        {
            // when running in Monolithic configuration, UI must provide the services for Razor-based templating (e.g. mail content generation)
            builder.AddRazorTemplating();
        }
    }
}
