using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using WebApp.UI.Infrastructure.Hosting;

namespace WebApp.UI
{
    public partial class Startup
    {
        internal const string ApiTenantId = "Api";

        private sealed class ApiTenant : Tenant
        {
            private readonly Api.Startup _apiStartup;

            public ApiTenant(string id, Startup startup, Assembly? entryAssembly = null)
                : base(id, startup.Configuration, startup.Environment, entryAssembly)
            {
                _apiStartup = startup.ApiStartup;
            }

            public override Func<HttpContext, bool>? BranchPredicate { get; } = CreatePathPrefixBranchPredicate("/api");

            protected override bool ShouldResolveFromRoot(ServiceDescriptor service) =>
                // AddDataAnnotationsLocalization calls AddLocalization under the hood, that is, it adds base localization services,
                // but those are already registered in the root container and we need those shared instances
                // https://github.com/dotnet/aspnetcore/blob/v3.1.18/src/Mvc/Mvc.Localization/src/MvcLocalizationServices.cs#L14
                service.ServiceType == typeof(IStringLocalizerFactory) || service.ServiceType == typeof(IStringLocalizer<>);

            public override void ConfigureServices(IServiceCollection services) => _apiStartup.ConfigureAppServices(services);

            public override void Configure(IApplicationBuilder app) => _apiStartup.Configure(app);
        }
    }
}
