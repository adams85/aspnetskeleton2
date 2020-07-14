using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp.UI
{
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
}
