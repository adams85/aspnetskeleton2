using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebApp.Core.Helpers;
using WebApp.UI.Infrastructure.Hosting;

namespace WebApp.UI
{
    public partial class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        partial void ConfigureServicesPartial(IServiceCollection services);

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // We want to run the API and MVC UI app on a single ASP.NET Core host but we cannot get along with a single DI container currently
            // as endpoint routing is bugged regarding pipeline branches (https://github.com/dotnet/aspnetcore/issues/19330),
            // so we need two application branches with isolated MVC-related and shared common services (like service layer services).
            // Unfortunately, the built-in DI is insufficient for this setup, we need some more advanced solution like Autofac.
            // As a matter of fact, Autofac could handle this situation out-of-the-box (https://autofaccn.readthedocs.io/en/latest/integration/aspnetcore.html#multitenant-support)
            // but this approach also has issues currently (https://github.com/autofac/Autofac.AspNetCore.Multitenant/issues/27)

            // The services defined here go into the root DI container and are accessible to the nested (tenant) containers.

            // 1. we need to remove the routing-related services because we don't want them to be shared between the tenants
            // (we don't have to retain the removed services because AddControllers, etc. will re-add them to the tenant containers eventually)
            var routingServices = new ServiceCollection();
            var routingAbstractionsAssembly = typeof(IRouter).Assembly;
            var routingAssembly = typeof(RouteBase).Assembly;
            services.RemoveAll(service => service.ServiceType.Assembly == routingAbstractionsAssembly || service.ServiceType.Assembly == routingAssembly);

            // 2. register other shared services

            var apiStartup = new Api.Startup(Configuration, Environment, provideRazorTemplating: false);

            apiStartup.ConfigureBaseServices(services);

            ConfigureServicesPartial(services);

            // 3. then register the tenants
            services.AddSingleton(new Tenants(
                new ApiTenant(ApiTenantId, apiStartup, typeof(Api.Startup).Assembly),
                new UITenant(UITenantId, apiStartup, typeof(Startup).Assembly)));

            // 4. finally register a startup filter which ensures that the main branch is set up before any other middleware added
            services.Insert(0, ServiceDescriptor.Transient<IStartupFilter, MainBranchSetupFilter>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Tenants tenants)
        {
            // Only general configuration here. For the UI tenant's pipeline see UITenant.Configure.

            var mainBranchTenant = tenants.MainBranchTenant;

            foreach (var tenant in tenants)
                if (tenant != mainBranchTenant)
                    app.MapWhen(tenant.BranchPredicate, branch =>
                    {
                        branch.ApplicationServices = tenant.TenantServices;

                        branch.UseMiddleware<BranchRequestServicesMiddleware>(tenant);

                        tenant.Configure(branch);
                    });

            mainBranchTenant?.Configure(app);
        }

        private sealed class MainBranchSetupFilter : IStartupFilter
        {
            private readonly Tenants _tenants;

            public MainBranchSetupFilter(Tenants tenants)
            {
                _tenants = tenants;
            }

            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
            {
                var mainBranchTenant = _tenants.MainBranchTenant;

                if (mainBranchTenant != null)
                {
                    app.ApplicationServices = mainBranchTenant.TenantServices;

                    app.UseMiddleware<BranchRequestServicesMiddleware>(mainBranchTenant);
                }

                next(app);
            };
        }

        private sealed class BranchRequestServicesMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly IServiceScopeFactory _serviceScopeFactory;
            private readonly IHttpContextAccessor? _httpContextAccessor;

            public BranchRequestServicesMiddleware(RequestDelegate next, Tenant tenant)
            {
                _next = next;
                _serviceScopeFactory = tenant.TenantServices.GetRequiredService<IServiceScopeFactory>();
                _httpContextAccessor = tenant.TenantServices.GetService<IHttpContextAccessor>();
            }

            public async Task Invoke(HttpContext context)
            {
                if (_httpContextAccessor != null && _httpContextAccessor.HttpContext == null)
                    _httpContextAccessor.HttpContext = context;

                IServiceProvidersFeature? originalServiceProvidersFeature = null;
                try
                {
                    var scope = _serviceScopeFactory.CreateScope();

                    if (scope is IAsyncDisposable asyncDisposable)
                        context.Response.RegisterForDisposeAsync(asyncDisposable);
                    else
                        context.Response.RegisterForDispose(scope);

                    originalServiceProvidersFeature = context.Features.Get<IServiceProvidersFeature>();

                    context.Features.Set<IServiceProvidersFeature>(new ServiceProvidersFeature { RequestServices = scope.ServiceProvider });

                    await _next(context);
                }
                finally { context.Features.Set(originalServiceProvidersFeature); }
            }
        }
    }
}
