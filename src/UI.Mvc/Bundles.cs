using System.Collections.Generic;
using System.Linq;
using Karambolo.AspNetCore.Bundling;
using Karambolo.AspNetCore.Bundling.Internal.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApp.UI.Infrastructure.Theming;

namespace WebApp.UI
{
    public class Bundles : DesignTimeBundlingConfiguration
    {
        // setup for run-time mode bundling
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment, UIOptions.BundleOptions options)
        {
            var bundling = services.AddBundling()
                .UseWebMarkupMin()
                .UseHashVersioning()
                .AddCss()
                .AddJs()
                .AddSass();

            if (options.UsePersistentCache)
            {
                bundling.UseFileSystemCaching();
                services.Configure<FileSystemBundleCacheOptions>(configuration.GetSection("Response:BundleCaching"));
            }
            else
                bundling.UseMemoryCaching();

            if (options.EnableResponseMinification)
                bundling.EnableMinification();

            if (options.EnableResponseCaching)
                bundling.EnableCacheHeader(options.CacheHeaderMaxAge ?? UIOptions.DefaultCacheHeaderMaxAge);

            if (environment.IsDevelopment())
                bundling.EnableChangeDetection();
        }

        public Bundles() { }

        // setup for design-time mode bundling
        public override IEnumerable<IBundlingModule> Modules => base.Modules.Concat(new IBundlingModule[]
        {
            new WebMarkupMinBundlingModule(),
            new SassBundlingModule()
        });

        public override void Configure(BundleCollectionConfigurer bundles)
        {
            var themeProvider =
                bundles.AppServices.GetService<IThemeProvider>() ??
                new ThemeProvider(bundles.AppServices.GetRequiredService<IWebHostEnvironment>());

            var themes = themeProvider.GetThemes();

            bundles.AddJs("/js/global.js")
                .Include("/js/*.js");

            bundles.AddJs("/js/dashboard.js")
                .Include("/js/dashboard/*.js");

            for (int i = 0, n = themes.Count; i < n; i++)
            {
                var sourcePath = themeProvider.GetThemePath(ThemeProvider.ThemesBasePath, themes[i]);
                var destPath = themeProvider.GetThemePath("/css", themes[i]);

                bundles.AddSass(destPath + "/global.css")
                    .Include(sourcePath + "/dashboard.scss");
            }
        }
    }
}
