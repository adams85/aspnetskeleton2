using System;
using System.ComponentModel;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebApp.Api.Infrastructure;
using WebApp.Api.Infrastructure.ErrorHandling;
using WebApp.Api.Infrastructure.UrlRewriting;
using WebApp.Core.Infrastructure;

namespace WebApp.Api
{
    public partial class Startup
    {
        private readonly bool _provideRazorTemplating;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
            : this(configuration, environment, provideRazorTemplating: true) { }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment, bool provideRazorTemplating)
        {
            Configuration = configuration;
            Environment = environment;

            _provideRazorTemplating = provideRazorTemplating;

            ApiOptions = new ApiOptions();
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public bool IsRunningBehindProxy { get; private set; }
        public ApiOptions ApiOptions { get; private set; }

        partial void ConfigureBaseServicesPartial(IServiceCollection services);

        public void ConfigureBaseServices(IServiceCollection services, IServiceProvider optionsProvider)
        {
            IsRunningBehindProxy = Configuration.GetValue("ForwardedHeaders_Enabled", false);
            ApiOptions = optionsProvider.GetRequiredService<IOptions<ApiOptions>>().Value;

            services.AddServiceLayer(optionsProvider);

            ConfigureOptions(services);

            services.AddScoped<IApplicationInitializer>(sp => new DelegatedApplicationInitializer(() =>
            {
                TypeDescriptor.AddAttributes(typeof(IPAddress), new TypeConverterAttribute(typeof(IPAddressTypeConverter)));

                ApiContractSerializer.TypeNameFormatterFactory = () => Core.Helpers.TypeExtensions.AssemblyQualifiedNameWithoutAssemblyDetails;
            }));

            if (IsRunningBehindProxy)
                services.Insert(0, ServiceDescriptor.Transient<IStartupFilter, PathAdjustmentStartupFilter>());

            ConfigureBaseServicesPartial(services);
        }

        partial void ConfigureAppServicesPartial(IServiceCollection services);

        public void ConfigureAppServices(IServiceCollection services)
        {
            ConfigureSecurityServices(services);

            if (ApiOptions.EnableSwagger)
                ConfigureSwaggerServices(services);

            ConfigureAppServicesPartial(services);

            var mvcBuilder = services.AddControllers();

            ConfigureMvc(mvcBuilder);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            using (var optionsProvider = BuildImmediateOptionsProvider())
                ConfigureBaseServices(services, optionsProvider);

            ConfigureAppServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<ApiErrorHandlerMiddleware>();

            if (!IsRunningBehindProxy)
                app.UseHttpsRedirection();

            if (ApiOptions.EnableSwagger)
                ConfigureSwagger(app);

            app.UseRouting();

            ConfigureSecurity(app);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        // https://andrewlock.net/exploring-istartupfilter-in-asp-net-core/
        private sealed class PathAdjustmentStartupFilter : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
            {
                var pathAdjusterOptions = app.ApplicationServices.GetRequiredService<IOptions<PathAdjusterOptions>>();

                if (pathAdjusterOptions.Value.PathAdjustments.Count > 0)
                    app.UseMiddleware<PathAdjusterMiddleware>(pathAdjusterOptions);

                next(app);
            };
        }
    }
}
