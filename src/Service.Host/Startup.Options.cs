using Microsoft.Extensions.DependencyInjection;

namespace WebApp.Service.Host;

public partial class Startup
{
    private ServiceProvider BuildImmediateOptionsProvider()
    {
        var optionsServices = new ServiceCollection().AddOptions();
        ConfigureImmediateOptions(optionsServices);
        return optionsServices.BuildServiceProvider();
    }

    partial void ConfigureServiceLayerImmediateOptionsPartial(IServiceCollection services);

    private void ConfigureImmediateOptions(IServiceCollection services)
    {
        ConfigureServiceLayerImmediateOptionsPartial(services);
    }

    partial void ConfigureServiceLayerOptionsPartial(IServiceCollection services);

    private void ConfigureOptions(IServiceCollection services)
    {
        ConfigureImmediateOptions(services);

        ConfigureServiceLayerOptionsPartial(services);
    }
}
