using Microsoft.Extensions.DependencyInjection;

namespace WebApp.UI;

public partial class Startup
{
    partial void ConfigureImmediateOptionsPartial(IServiceCollection services);

    private void ConfigureImmediateOptions(IServiceCollection services)
    {
        ConfigureImmediateOptionsPartial(services);

        services.Configure<UIOptions>(Configuration.GetSection(UIOptions.DefaultSectionName));
    }

    partial void ConfigureOptionsPartial(IServiceCollection services);

    private void ConfigureOptions(IServiceCollection services)
    {
        ConfigureImmediateOptions(services);

        ConfigureOptionsPartial(services);
    }
}
