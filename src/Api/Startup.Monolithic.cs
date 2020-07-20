using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebApp.Api.Infrastructure.Localization;

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
                    .AddRazorTemplating()
                    .AddViewLocalization();

                services
                    .ReplaceLast(ServiceDescriptor.Singleton<IHtmlLocalizerFactory, ExtendedHtmlLocalizerFactory>())
                    .ReplaceLast(ServiceDescriptor.Singleton<IViewLocalizer, ExtendedViewLocalizer>());
            }
        }
    }
}
