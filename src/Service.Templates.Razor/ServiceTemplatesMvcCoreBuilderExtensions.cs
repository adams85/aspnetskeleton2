using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.FileProviders;
using WebApp.Service.Infrastructure.Templating;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceTemplatesServiceCollectionExtensions
    {
        private static IServiceCollection AddRazorTemplating(this IServiceCollection services)
        {
            services.PostConfigure<MvcRazorRuntimeCompilationOptions>(options =>
            {
                // the templates are copied to the output directory, so we need to make sure that the Razor engine finds them

                var appBaseDirectoryNormalizedPath = Path.GetFullPath(AppBaseDirectoryFileProvider.Instance.Root);
                var pathComparer = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

                if (!options.FileProviders.Any(fileProvider =>
                    fileProvider is PhysicalFileProvider physicalFileProvider &&
                    Path.TrimEndingDirectorySeparator(Path.GetFullPath(physicalFileProvider.Root).AsSpan())
                        .Equals(Path.TrimEndingDirectorySeparator(appBaseDirectoryNormalizedPath.AsSpan()), pathComparer)))
                {
                    options.FileProviders.Add(AppBaseDirectoryFileProvider.Instance);
                }
            });

            services.AddTransient<ITemplateRenderer, RazorTemplateRenderer>();

            return services;
        }

        public static IMvcCoreBuilder AddRazorTemplating(this IMvcCoreBuilder builder)
        {
            builder.AddRazorViewEngine();

            builder.Services.AddRazorTemplating();

            return builder;
        }

        /// <remarks>
        /// Use only when MVC services was configured to use Razor (e.g. AddControllersWithViews)
        /// because this overload is unable to add the Razor view engine services.
        /// </remarks>
        public static IMvcBuilder AddRazorTemplating(this IMvcBuilder builder)
        {
            builder.Services.AddRazorTemplating();

            return builder;
        }

        private sealed class AppBaseDirectoryFileProvider : PhysicalFileProvider
        {
            public static readonly AppBaseDirectoryFileProvider Instance = new AppBaseDirectoryFileProvider();

            private AppBaseDirectoryFileProvider() : base(AppContext.BaseDirectory) { }
        }
    }
}
