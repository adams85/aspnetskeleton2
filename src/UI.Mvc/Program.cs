using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Karambolo.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApp.Core.Helpers;
using WebApp.UI.Infrastructure.Hosting;

namespace WebApp.UI
{
    public partial class Program
    {
        public static readonly string ApplicationName =
            typeof(Program).GetAttributes<AssemblyProductAttribute>().FirstOrDefault()?.Product ?? typeof(Program).Assembly.GetName().Name!;

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.InitializeApplicationAsync();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new MultitenantServiceProviderFactory())
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureLogging(ConfigureLogging)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseWindowsService()
                .UseSystemd();

        static partial void ConfigureAppConfigurationPartial(HostBuilderContext context, IConfigurationBuilder builder, IFileProvider fileProvider, ref int index);

        private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
        {
            const string overrideSettingsTag = "UI";

            var env = context.HostingEnvironment;

            var fileProvider = builder.GetFileProvider();
            if (!fileProvider.GetFileInfo("appsettings.json").Exists)
                fileProvider = new PhysicalFileProvider(AppContext.BaseDirectory);

            int index = builder.Sources.Count;
            builder.Sources.RemoveAll((source, i) => source is JsonConfigurationSource ? (index = Math.Min(index, i), @true: true).@true : false);

            ConfigureAppConfigurationPartial(context, builder, fileProvider, ref index);

            // adding shared configuration files which are linked from Api project
            builder.Sources.Insert(index++, new JsonConfigurationSource { FileProvider = fileProvider, Path = "appsettings.json", Optional = true, ReloadOnChange = true });
            builder.Sources.Insert(index++, new JsonConfigurationSource { FileProvider = fileProvider, Path = $"appsettings.{env.EnvironmentName}.json", Optional = true, ReloadOnChange = true });
            builder.Sources.Insert(index++, new JsonConfigurationSource { FileProvider = fileProvider, Path = $"appsettings.{Architecture}.json", Optional = true, ReloadOnChange = true });
            builder.Sources.Insert(index++, new JsonConfigurationSource { FileProvider = fileProvider, Path = $"appsettings.{Architecture}.{env.EnvironmentName}.json", Optional = true, ReloadOnChange = true });

            builder.Sources.Insert(index++, new JsonConfigurationSource { Path = $"appsettings.{overrideSettingsTag}.json", Optional = true, ReloadOnChange = true });
            builder.Sources.Insert(index++, new JsonConfigurationSource { Path = $"appsettings.{overrideSettingsTag}.{env.EnvironmentName}.json", Optional = true, ReloadOnChange = true });
            builder.Sources.Insert(index++, new JsonConfigurationSource { Path = $"appsettings.{overrideSettingsTag}.{Architecture}.json", Optional = true, ReloadOnChange = true });
            builder.Sources.Insert(index++, new JsonConfigurationSource { Path = $"appsettings.{overrideSettingsTag}.{Architecture}.{env.EnvironmentName}.json", Optional = true, ReloadOnChange = true });
        }

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        {
            builder
                .ClearProviders()
                .AddConsole()
                .AddFile(options => options.RootPath = context.HostingEnvironment.ContentRootPath);

            if (context.HostingEnvironment.IsDevelopment())
                builder.AddDebug();
        }
    }
}
