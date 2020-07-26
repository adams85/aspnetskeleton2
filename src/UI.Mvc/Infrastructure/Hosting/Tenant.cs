using System;
using System.Reflection;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting.Internal;
using WebApp.Core.Helpers;

namespace WebApp.UI.Infrastructure.Hosting
{
    public abstract class Tenant : IDisposable, IAsyncDisposable
    {
        protected static Func<HttpContext, bool> CreatePathPrefixBranchPredicate(PathString prefix) => ctx =>
        {
            var request = ctx.Request;

            if (!request.Path.StartsWithSegments(prefix, out var remaining))
                return false;

            request.PathBase += prefix;
            request.Path = remaining;
            return true;
        };

        public Tenant(string id, IConfiguration configuration, IWebHostEnvironment environment, Assembly? entryAssembly = null)
        {
            Id = id;
            Configuration = configuration;
            Environment = environment;
            EntryAssembly = entryAssembly;
        }

        public void Dispose()
        {
            if (TenantServices != null)
            {
                TenantServices.Dispose();
                TenantServices = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (TenantServices != null)
            {
                await TenantServices.DisposeAsync();
                TenantServices = null;
            }
        }

        public string Id { get; }
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }
        public Assembly? EntryAssembly { get; }

        public abstract Func<HttpContext, bool>? BranchPredicate { get; }

        public AutofacServiceProvider? TenantServices { get; private set; }

        public abstract void ConfigureServices(IServiceCollection services);

        protected virtual bool ShouldResolveFromRoot(ServiceDescriptor service) => false;

        public void InitializeServices(AutofacServiceProvider rootServices)
        {
            TenantServices ??= new AutofacServiceProvider(rootServices.LifetimeScope.BeginLifetimeScope(builder =>
            {
                var services = new ServiceCollection();

                // HACK: IWebHostEnvironment is registered in the root container but we need to register a temporary instance for the time of tenant services configuration
                // because discovery of default application parts relies on IWebHostEnvironment.ApplicationName
                // https://github.com/dotnet/aspnetcore/blob/v3.1.6/src/Mvc/Mvc.Core/src/DependencyInjection/MvcCoreServiceCollectionExtensions.cs#L81
                var dummyWebHostEnvironmentService = ServiceDescriptor.Singleton<IWebHostEnvironment>(new DummyWebHostEnvironment { ApplicationName = EntryAssembly?.GetName().Name });

                services.Add(dummyWebHostEnvironmentService);

                ConfigureServices(services);

                services.RemoveAll((service, _) => service == dummyWebHostEnvironmentService || ShouldResolveFromRoot(service));

                builder.Populate(services);
            }));
        }

        public abstract void Configure(IApplicationBuilder app);

        private sealed class DummyWebHostEnvironment : HostingEnvironment, IWebHostEnvironment
        {
            public IFileProvider? WebRootFileProvider { get; set; }
            public string? WebRootPath { get; set; }
        }
    }
}
