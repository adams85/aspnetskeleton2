using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProtoBuf.Grpc.Server;
using WebApp.Api.Infrastructure.Localization;
using WebApp.Service.Host.Infrastructure;
using WebApp.Service.Host.Infrastructure.Localization;
using WebApp.Service.Host.Services;
using WebApp.Service.Infrastructure;

namespace WebApp.Service.Host;

public partial class Startup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    public IConfiguration Configuration { get; }
    public IWebHostEnvironment Environment { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        using (var optionsProvider = BuildImmediateOptionsProvider())
            services.AddServiceLayer(optionsProvider);

        services
            .ReplaceLast(ServiceDescriptor.Singleton<IExecutionContextAccessor, HttpExecutionContextAccessor>())
            .AddHttpContextAccessor();

        ConfigureOptions(services);

        services.AddMvcCore()
            .AddRazorTemplating()
            .AddViewLocalization();

        services
            .ReplaceLast(ServiceDescriptor.Singleton<IHtmlLocalizerFactory, ExtendedHtmlLocalizerFactory>())
            .ReplaceLast(ServiceDescriptor.Transient<IViewLocalizer, ExtendedViewLocalizer>());

        // https://protobuf-net.github.io/protobuf-net.Grpc/gettingstarted
        services.AddCodeFirstGrpc(options => options.Interceptors.Add<RestoreExecutionContextInterceptor>());
        services.AddSingleton(_ => ServiceHostContractSerializer.CreateBinderConfiguration());
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<CommandService>();
            endpoints.MapGrpcService<QueryService>();
        });
    }
}
