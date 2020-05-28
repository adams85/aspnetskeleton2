using System;
using Microsoft.Extensions.Localization;
using ProtoBuf.Grpc.Client;
using WebApp.Core.Infrastructure;
using WebApp.Service.Infrastructure;
using WebApp.Service.Infrastructure.Localization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceProxyServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceLayer(this IServiceCollection services, IServiceProvider optionsProvider)
        {
            services.AddCoreServices();

            services.AddApplicationInitializers();

            // TODO: implement localization
            services
                .AddSingleton<IStringLocalizerFactory>(NullStringLocalizerFactory.Instance)
                .AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

            services.AddSingleton<IServiceHostGrpcServiceFactory, ServiceHostGrpcServiceFactory>();
            services.AddSingleton<IQueryDispatcher, ServiceHostQueryDispatcher>();
            services.AddSingleton<ICommandDispatcher, ServiceHostCommandDispatcher>();

            return services;
        }

        private static IServiceCollection AddApplicationInitializers(this IServiceCollection services)
        {
            services.AddScoped<IApplicationInitializer>(sp => new DelegatedApplicationInitializer(() =>
            {
                GrpcClientFactory.AllowUnencryptedHttp2 = true;
            }));

            return services;
        }
    }
}
