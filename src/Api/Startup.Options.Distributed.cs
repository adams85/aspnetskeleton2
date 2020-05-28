using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Service;

namespace WebApp.Api
{
    public partial class Startup
    {
        partial void ConfigureImmediateOptionsPartial(IServiceCollection services)
        {
            services.Configure<ServiceProxyApplicationOptions>(Configuration.GetSection(ServiceProxyApplicationOptions.DefaultSectionName));
        }
    }
}
