using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using ProtoBuf.Grpc.Client;
using WebApp.Core.Infrastructure;
using WebApp.Service.Infrastructure;
using WebApp.Service.Infrastructure.Events;
using WebApp.Service.Infrastructure.Localization;
using WebApp.Service.Settings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceProxyServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceLayer(this IServiceCollection services, IServiceProvider optionsProvider)
        {
            services.AddCoreServices();

            services.AddSingleton<IEventListener, ServiceHostEventListener>();

            services.AddSingleton<ISettingsProvider, SettingsProvider>();

            // TODO: implement localization
            services
                .AddSingleton<IStringLocalizerFactory>(NullStringLocalizerFactory.Instance)
                .AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

            services.AddApplicationInitializers();

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

            services.AddScoped<IApplicationInitializer>(sp => new DelegatedApplicationInitializer(_ =>
            {
                var settingsProvider = sp.GetRequiredService<ISettingsProvider>();

                return Task.WhenAll(settingsProvider.Initialization);
            }));

            return services;
        }
    }
}
