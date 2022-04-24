using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApp.Core.Helpers;

namespace WebApp.Service.Host;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await host.InitializeApplicationAsync();
        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(ConfigureAppConfiguration)
            .ConfigureLogging(ConfigureLogging)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .ConfigureKestrel(options => options.ConfigureEndpointDefaults(listenOptions => listenOptions.Protocols = HttpProtocols.Http2))
                    .UseStartup(context => new Startup(context.Configuration, context.HostingEnvironment));
            })
            .UseWindowsService()
            .UseSystemd();

    private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
    {
        const string serviceSettingsTag = "Service";

        var env = context.HostingEnvironment;

        var fileProvider = builder.GetFileProvider();
        if (!fileProvider.GetFileInfo($"appsettings.{serviceSettingsTag}.json").Exists)
            fileProvider = new PhysicalFileProvider(AppContext.BaseDirectory);

        int index = builder.Sources.Count;
        builder.Sources.RemoveAll((source, i) => source is JsonConfigurationSource ? (index = Math.Min(index, i), @true: true).@true : false);

        builder.Sources.Insert(index++, new JsonConfigurationSource { FileProvider = fileProvider, Path = $"appsettings.{serviceSettingsTag}.json", Optional = true, ReloadOnChange = true });
        builder.Sources.Insert(index++, new JsonConfigurationSource { FileProvider = fileProvider, Path = $"appsettings.{serviceSettingsTag}.{env.EnvironmentName}.json", Optional = true, ReloadOnChange = true });
        builder.Sources.Insert(index++, new JsonConfigurationSource { Path = "appsettings.json", Optional = true, ReloadOnChange = true });
        builder.Sources.Insert(index++, new JsonConfigurationSource { Path = $"appsettings.{env.EnvironmentName}.json", Optional = true, ReloadOnChange = true });
    }

    private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
    {
        builder
            .ClearProviders()
            .AddConsole()
            .AddJsonFile(options => options.RootPath = context.HostingEnvironment.ContentRootPath);

        if (context.HostingEnvironment.IsDevelopment())
            builder.AddDebug();
    }
}
