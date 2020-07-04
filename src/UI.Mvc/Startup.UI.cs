using System;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using WebApp.Core.Helpers;
using WebApp.UI.Infrastructure.Hosting;

namespace WebApp.UI
{
    public partial class Startup
    {
        internal const string UITenantId = "UI";

        private sealed partial class UITenant : Tenant
        {
            private readonly Api.Startup _apiStartup;

            public UITenant(string id, Api.Startup apiStartup, Assembly? entryAssembly = null)
                : base(id, apiStartup.Configuration, apiStartup.Environment, entryAssembly)
            {
                _apiStartup = apiStartup;
            }

            public bool IsRunningBehindProxy => _apiStartup.IsRunningBehindProxy;

            public override Func<HttpContext, bool>? BranchPredicate => null;

            partial void ConfigureMvcPartial(IMvcBuilder builder);

            public override void ConfigureServices(IServiceCollection services)
            {
                services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie();

                var mvcBuilder = services.AddControllersWithViews()
                    .ConfigureApplicationPartManager(manager =>
                    {
                        // ApplicationPartManager finds the Api assembly (as it is referenced), so we have to exclude it manually
                        // because we don't want the API controllers to be visible to the UI branch
                        // https://github.com/dotnet/aspnetcore/blob/v3.1.5/src/Mvc/Mvc.Core/src/DependencyInjection/MvcCoreServiceCollectionExtensions.cs#L81
                        manager.ApplicationParts.RemoveAll((part, _) => part is AssemblyPart assemblyPart && assemblyPart.Assembly == typeof(Api.Startup).Assembly);
                    });

                // we avoid AddViewLocalization here because it calls AddLocalization under the hood, that is, it would add base localization services,
                // but they are already registered in the root container and we need those shared instances
                // https://github.com/dotnet/aspnetcore/blob/v3.1.5/src/Mvc/Mvc.Localization/src/MvcLocalizationServices.cs#L36
                services.Configure<RazorViewEngineOptions>(options => options.ViewLocationExpanders.Add(new LanguageViewLocationExpander(LanguageViewLocationExpanderFormat.Suffix)));
                services.TryAdd(ServiceDescriptor.Singleton<IHtmlLocalizerFactory, HtmlLocalizerFactory>());
                services.TryAdd(ServiceDescriptor.Transient(typeof(IHtmlLocalizer<>), typeof(HtmlLocalizer<>)));
                services.TryAdd(ServiceDescriptor.Transient<IViewLocalizer, ViewLocalizer>());

                _apiStartup.ConfigureModelServices(mvcBuilder);

#if !RAZOR_PRECOMPILE
                mvcBuilder.AddRazorRuntimeCompilation();
#endif

                ConfigureMvcPartial(mvcBuilder);
            }

            public override void Configure(IApplicationBuilder app)
            {
                if (Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                if (!IsRunningBehindProxy)
                    app.UseHttpsRedirection();

                app.UseStaticFiles();

                app.UseRouting();

                app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");
                });
            }
        }
    }
}
