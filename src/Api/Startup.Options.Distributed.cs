using Microsoft.Extensions.DependencyInjection;
using WebApp.Service;

namespace WebApp.Api;

public partial class Startup
{
    partial void ConfigureImmediateOptionsPartial(IServiceCollection services)
    {
        services.Configure<ApplicationOptions>(Configuration.GetSection(ApplicationOptions.DefaultSectionName));
        services.Configure<ServiceProxyApplicationOptions>(Configuration.GetSection(ApplicationOptions.DefaultSectionName));
    }
}
