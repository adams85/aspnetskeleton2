using WebApp.Service.Infrastructure.Templating;

namespace Microsoft.Extensions.DependencyInjection
{
#if !RAZOR_PRECOMPILE
    using System;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
    using Microsoft.Extensions.FileProviders;
#endif

    // Set RazorCompileOnBuild property to true in the csproj file to enable precompilation of templates.

    public static class ServiceTemplatesServiceCollectionExtensions
    {
        private static IServiceCollection AddRazorTemplating(this IServiceCollection services)
        {
#if !RAZOR_PRECOMPILE
            services.AddOptions<MvcRazorRuntimeCompilationOptions>()
                .Configure<IWebHostEnvironment>((options, env) => options.FileProviders.Add(new PhysicalFileProvider(AppContext.BaseDirectory)));
#endif

            services.AddTransient<ITemplateRenderer, RazorTemplateRenderer>();

            return services;
        }

        public static IMvcCoreBuilder AddRazorTemplating(this IMvcCoreBuilder builder)
        {
            builder.AddRazorViewEngine();

#if !RAZOR_PRECOMPILE
            builder.AddRazorRuntimeCompilation();
#endif

            builder.Services.AddRazorTemplating();

            return builder;
        }

        /// <remarks>
        /// Use only when MVC services was configured to use Razor (e.g. AddControllersWithViews)
        /// because this overload is unable to add the Razor view engine services.
        /// </remarks>
        public static IMvcBuilder AddRazorTemplating(this IMvcBuilder builder)
        {
#if !RAZOR_PRECOMPILE
            builder.AddRazorRuntimeCompilation();
#endif

            builder.Services.AddRazorTemplating();

            return builder;
        }
    }
}
