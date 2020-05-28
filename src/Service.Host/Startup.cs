using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProtoBuf.Grpc.Configuration;
using ProtoBuf.Grpc.Server;
using WebApp.DataAccess;
using WebApp.Service.Host.Services;
using WebApp.Service.Mailing;

namespace WebApp.Service.Host
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

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            using (var optionsProvider = BuildImmediateOptionsProvider())
                services.AddServiceLayer(optionsProvider);

            ConfigureOptions(services);

            services.AddMvcCore()
                .AddRazorTemplating();

            // https://protobuf-net.github.io/protobuf-net.Grpc/gettingstarted
            services.AddCodeFirstGrpc();
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
}
