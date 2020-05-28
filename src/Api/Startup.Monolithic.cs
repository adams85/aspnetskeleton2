using Microsoft.Extensions.DependencyInjection;

namespace WebApp.Api
{
    public partial class Startup
    {
        partial void ConfigureAppServicesPartial(IServiceCollection services)
        {
            if (_provideRazorTemplating)
            {
                // when running standalone in Monolithic configuration, Api must provide the services for Razor-based templating (e.g. mail content generation)
                services.AddMvcCore()
                    .AddRazorTemplating();
            }
        }
    }
}
