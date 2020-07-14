using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Api.Infrastructure.Security;
using WebApp.Api.Infrastructure.UrlRewriting;

namespace WebApp.Api
{
    public partial class Startup
    {
        public ServiceProvider BuildImmediateOptionsProvider(Action<IServiceCollection>? configure = null)
        {
            var optionsServices = new ServiceCollection().AddOptions();
            ConfigureImmediateOptions(optionsServices);
            configure?.Invoke(optionsServices);
            return optionsServices.BuildServiceProvider();
        }

        partial void ConfigureServiceLayerImmediateOptionsPartial(IServiceCollection services);
        partial void ConfigureImmediateOptionsPartial(IServiceCollection services);

        private void ConfigureImmediateOptions(IServiceCollection services)
        {
            ConfigureServiceLayerImmediateOptionsPartial(services);
            ConfigureImmediateOptionsPartial(services);

            services.Configure<ApiOptions>(Configuration.GetSection(ApiOptions.DefaultSectionName));
        }

        partial void ConfigureServiceLayerOptionsPartial(IServiceCollection services);
        partial void ConfigureOptionsPartial(IServiceCollection services);

        private void ConfigureOptions(IServiceCollection services)
        {
            ConfigureImmediateOptions(services);

            ConfigureServiceLayerOptionsPartial(services);
            ConfigureOptionsPartial(services);

            services.Configure<ApiSecurityOptions>(Configuration.GetSection(ApiSecurityOptions.DefaultSectionName));

            if (IsRunningBehindProxy)
            {
                services.Configure<ForwardedHeadersOptions>(Configuration.GetSection("ForwardedHeaders"));
                services.Configure<PathAdjusterOptions>(Configuration.GetSection(PathAdjusterOptions.DefaultSectionName));
            }
        }
    }
}
