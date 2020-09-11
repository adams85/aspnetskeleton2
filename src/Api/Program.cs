using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Karambolo.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApp.Core.Helpers;

namespace WebApp.Api
{
    public partial class Program
    {
        public static readonly string ApplicationName =
            typeof(Program).Assembly.GetAttributes<AssemblyProductAttribute>().FirstOrDefault()?.Product ??
            typeof(Program).Assembly.GetName().Name!;

        public static readonly string ApplicationVersion =
            typeof(Program).Assembly.GetAttributes<AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion ??
            typeof(Program).Assembly.GetName().Version!.ToString();

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.InitializeApplicationAsync();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureLogging(ConfigureLogging)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseWindowsService()
                .UseSystemd();

        static partial void ConfigureAppConfigurationPartial(HostBuilderContext context, IConfigurationBuilder builder, ref int index);

        private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
        {
            var env = context.HostingEnvironment;

            int index = builder.Sources.Count;
            builder.Sources.RemoveAll((source, i) => source is JsonConfigurationSource ? (index = Math.Min(index, i), @true: true).@true : false);

            ConfigureAppConfigurationPartial(context, builder, ref index);

            builder.Sources.Insert(index++, new JsonConfigurationSource { Path = "appsettings.json", Optional = true, ReloadOnChange = true });
            builder.Sources.Insert(index++, new JsonConfigurationSource { Path = $"appsettings.{env.EnvironmentName}.json", Optional = true, ReloadOnChange = true });
            builder.Sources.Insert(index++, new JsonConfigurationSource { Path = $"appsettings.{Architecture}.json", Optional = true, ReloadOnChange = true });
            builder.Sources.Insert(index++, new JsonConfigurationSource { Path = $"appsettings.{Architecture}.{env.EnvironmentName}.json", Optional = true, ReloadOnChange = true });
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
