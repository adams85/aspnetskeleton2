using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WebApp.UI.Infrastructure.Hosting;

namespace WebApp.UI
{
    public partial class Startup
    {
        internal const string ApiTenantId = "Api";

        private sealed class ApiTenant : Tenant
        {
            private readonly Api.Startup _apiStartup;

            public ApiTenant(string id, Api.Startup apiStartup, Assembly? entryAssembly = null)
                : base(id, apiStartup.Configuration, apiStartup.Environment, entryAssembly)
            {
                _apiStartup = apiStartup;
            }

            public override Func<HttpContext, bool>? BranchPredicate { get; } = CreatePathPrefixBranchPredicate("/api");

            public override void ConfigureServices(IServiceCollection services) => _apiStartup.ConfigureAppServices(services);

            public override void Configure(IApplicationBuilder app) => _apiStartup.Configure(app);
        }
    }
}
