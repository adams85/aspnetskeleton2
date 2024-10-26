using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Karambolo.Common;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApp.Core.Helpers;
using WebApp.UI.Infrastructure.Hosting;

namespace WebApp.UI;

public partial class Program
{
    public static readonly string ApplicationName =
        typeof(Program).Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ??
        typeof(Program).Assembly.GetName().Name!;

    public static readonly string ApplicationVersion =
        typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ??
        typeof(Program).Assembly.GetName().Version!.ToString();

    public static readonly bool UsesDesignTimeBundling =
#if USES_DESIGNTIME_BUNDLING
        true;
#else
        false;
#endif

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
            .ConfigureWebHost(webBuilder =>
            {
                ConfigureWebDefaults(webBuilder);
                FixHostingStartupAssemblies(webBuilder);
                webBuilder.UseStartup(context => new Startup(context.Configuration, context.HostingEnvironment));
            })
            .UseWindowsService()
            .UseSystemd();

    /// <remarks>
    /// This method should be identical to <see cref="WebHost"/>'s internal ConfigureWebDefaults method except that
    /// the call to <see cref="RoutingServiceCollectionExtensions.AddRouting(IServiceCollection)"/> must be removed as
    /// we need to prevent routing services from being added to the root container (see <seealso cref="Startup.ConfigureServices(IServiceCollection)"/> for details).
    /// </remarks>
    // based on: https://github.com/dotnet/aspnetcore/blob/v6.0.3/src/DefaultBuilder/src/WebHost.cs#L215
    private static void ConfigureWebDefaults(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((ctx, cb) =>
        {
            if (ctx.HostingEnvironment.IsDevelopment())
            {
                StaticWebAssetsLoader.UseStaticWebAssets(ctx.HostingEnvironment, ctx.Configuration);
            }
        });
        builder.UseKestrel((builderContext, options) =>
        {
            options.Configure(builderContext.Configuration.GetSection("Kestrel"), reloadOnChange: true);
        })
        .ConfigureServices((hostingContext, services) =>
        {
            // Fallback
            services.PostConfigure<HostFilteringOptions>(options =>
            {
                if (options.AllowedHosts == null || options.AllowedHosts.Count == 0)
                {
                    // "AllowedHosts": "localhost;127.0.0.1;[::1]"
                    var hosts = hostingContext.Configuration["AllowedHosts"]?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    // Fall back to "*" to disable.
                    options.AllowedHosts = (hosts?.Length > 0 ? hosts : new[] { "*" });
                }
            });
            // Change notification
            services.AddSingleton<IOptionsChangeTokenSource<HostFilteringOptions>>(new ConfigurationChangeTokenSource<HostFilteringOptions>(hostingContext.Configuration));

            var webHostAssembly = typeof(WebHost).Assembly;
            var hostFilteringStartupFilterType = Type.GetType("Microsoft.AspNetCore.HostFilteringStartupFilter, " + webHostAssembly.FullName, throwOnError: true)!;
            services.AddTransient(typeof(IStartupFilter), hostFilteringStartupFilterType);

            var forwardedHeadersStartupFilterType = Type.GetType("Microsoft.AspNetCore.ForwardedHeadersStartupFilter, " + webHostAssembly.FullName, throwOnError: true)!;
            services.AddTransient(typeof(IStartupFilter), forwardedHeadersStartupFilterType);

            services.AddOptions<ForwardedHeadersOptions>()
                // based on: https://github.com/dotnet/aspnetcore/blob/v6.0.3/src/DefaultBuilder/src/ForwardedHeadersOptionsSetup.cs
                .Configure<IConfiguration>((options, configuration) =>
                {
                    if (!string.Equals("true", configuration["ForwardedHeaders_Enabled"], StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                    // Only loopback proxies are allowed by default. Clear that restriction because forwarders are
                    // being enabled by explicit configuration.
                    options.KnownNetworks.Clear();
                    options.KnownProxies.Clear();
                });
        })
        .UseIIS()
        .UseIISIntegration();
    }

    /// <remarks>
    /// The <see href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/platform-specific-configuration?view=aspnetcore-6.0">hosting startup assemblies</see> feature
    /// does not play well with the project's multi-tenant, isolated IoC container setup in the case of all hosting startup assemblies because configuration done by startup assemblies
    /// affects the root container. When this is not desirable (like in the case of Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation), we have to prevent it from being registered
    /// by the framework and configure the related services manually in the <see cref="Tenant.ConfigureServices(IServiceCollection)"/> method.
    /// </remarks>
    private static void FixHostingStartupAssemblies(IWebHostBuilder builder)
    {
        var hostingStartupAssemblies = SplitAssemblyList(builder.GetSetting(WebHostDefaults.HostingStartupAssembliesKey)!)
            .ToLookup(value => new AssemblyName(value).Name, CachedDelegates.Identity<string>.Func, StringComparer.OrdinalIgnoreCase);

        IEnumerable<string> assembliesToExclude = Enumerable.Empty<string>();

        var assemblies = hostingStartupAssemblies["Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation"];
        if (assemblies.Any())
        {
            assembliesToExclude = assembliesToExclude.Concat(assemblies);
            builder.UseSetting(UIOptions.DefaultSectionName + ":" + nameof(UIOptions.EnableRazorRuntimeCompilation), bool.TrueString);
        }

        if (assembliesToExclude.Any())
        {
            assembliesToExclude = assembliesToExclude.Append(builder.GetSetting(WebHostDefaults.HostingStartupExcludeAssembliesKey)!);
            builder.UseSetting(WebHostDefaults.HostingStartupExcludeAssembliesKey, string.Join(';', assembliesToExclude));
        }

        static IEnumerable<string> SplitAssemblyList(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                yield break;

            foreach (var part in value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmedPart = part.Trim();
                if (!string.IsNullOrEmpty(trimmedPart))
                    yield return trimmedPart;
            }
        }
    }

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
            .AddJsonFile(options => options.RootPath = context.HostingEnvironment.ContentRootPath);

        if (context.HostingEnvironment.IsDevelopment())
            builder.AddDebug();
    }
}
