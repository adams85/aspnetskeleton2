using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using WebApp.Core.Helpers;
using WebApp.Service.Settings;
using WebApp.UI.Infrastructure.Hosting;
using WebApp.UI.Infrastructure.Localization;
using WebApp.UI.Infrastructure.Security;
using WebApp.UI.Infrastructure.Theming;
using WebApp.UI.Middlewares;
using WebMarkupMin.AspNetCore3;

namespace WebApp.UI
{
    public partial class Startup
    {
        internal const string UITenantId = "UI";

        private sealed partial class UITenant : Tenant
        {
            private readonly Api.Startup _apiStartup;

            public UITenant(string id, Startup startup, Assembly? entryAssembly = null)
                : base(id, startup.Configuration, startup.Environment, entryAssembly)
            {
                _apiStartup = startup.ApiStartup;

                IsRunningBehindProxy = _apiStartup.IsRunningBehindProxy;
                UIOptions = startup.UIOptions!;
            }

            public bool IsRunningBehindProxy { get; }
            public UIOptions UIOptions { get; }

            public override Func<HttpContext, bool>? BranchPredicate => null;

            partial void ConfigureMvcPartial(IMvcBuilder builder);

            public override void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton<IAccountManager, AccountManager>();

                services
                    .AddSingleton<IThemeProvider, ThemeProvider>()
                    .AddSingleton<IThemeManager, ThemeManager>();

                #region Response compression & minification

                if (UIOptions.Views.EnableResponseMinification || UIOptions.EnableResponseCompression)
                {
                    var webMarkupMin = services.AddWebMarkupMin(options =>
                    {
                        options.AllowCompressionInDevelopmentEnvironment = true;
                        options.AllowMinificationInDevelopmentEnvironment = true;
                        options.DisablePoweredByHttpHeaders = true;
                    });

                    if (UIOptions.Views.EnableResponseMinification)
                    {
                        webMarkupMin.AddHtmlMinification(o => o.SupportedMediaTypes = new HashSet<string>() { "text/html" });
                        services.Configure<HtmlMinificationOptions>(Configuration.GetSection("Response:HtmlMinification"));
                    }

                    if (UIOptions.EnableResponseCompression)
                    {
                        webMarkupMin.AddHttpCompression();
                        services.Configure<HttpCompressionOptions>(Configuration.GetSection("Response:HttpCompression"));
                    }
                }

                #endregion

                #region Bundling

                Bundles.ConfigureServices(services, Configuration, Environment, UIOptions.Bundles);

                #endregion

                #region Security

                // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie
                services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie();

                services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
                    .Bind(Configuration.GetSection($"{UISecurityOptions.DefaultSectionName}:Authentication"))
                    .Configure<IAccountManager>((options, accountManager) => options.Events = new UICookieAuthenticationEvents(accountManager));

                #endregion

                #region View caching

                if (UIOptions.Views.EnableResponseCaching)
                {
                    services.AddResponseCaching();
                    services.Configure<ResponseCachingOptions>(Configuration.GetSection("Response:ViewCaching"));
                }

                #endregion

                #region MVC

                var mvcBuilder = services.AddControllersWithViews()
                    .ConfigureApplicationPartManager(manager =>
                    {
                        // ApplicationPartManager finds the Api assembly (as it is referenced), so we have to exclude it manually
                        // because we don't want the API controllers to be visible to the UI branch
                        // https://github.com/dotnet/aspnetcore/blob/v3.1.5/src/Mvc/Mvc.Core/src/DependencyInjection/MvcCoreServiceCollectionExtensions.cs#L81
                        manager.ApplicationParts.RemoveAll((part, _) => part is AssemblyPart assemblyPart && assemblyPart.Assembly == typeof(Api.Startup).Assembly);
                    });

                // we avoid AddViewLocalization here because it calls AddLocalization under the hood, that is, it would add base localization services,
                // but those are already registered in the root container and we need to keep them shared
                // https://github.com/dotnet/aspnetcore/blob/v3.1.5/src/Mvc/Mvc.Localization/src/MvcLocalizationServices.cs#L36
                services.Configure<RazorViewEngineOptions>(options => options.ViewLocationExpanders.Add(new LanguageViewLocationExpander(LanguageViewLocationExpanderFormat.Suffix)));
                services.TryAdd(ServiceDescriptor.Singleton<IHtmlLocalizerFactory, ExtendedHtmlLocalizerFactory>());
                services.TryAdd(ServiceDescriptor.Transient(typeof(IHtmlLocalizer<>), typeof(HtmlLocalizer<>)));
                services.TryAdd(ServiceDescriptor.Transient<IViewLocalizer, ViewLocalizer>());

                _apiStartup.ConfigureDataAnnotationServices(mvcBuilder);

#if !RAZOR_PRECOMPILE
                mvcBuilder.AddRazorRuntimeCompilation();
#endif

                #endregion

                ConfigureMvcPartial(mvcBuilder);
            }

            public override void Configure(IApplicationBuilder app)
            {
                var settingsProvider = app.ApplicationServices.GetRequiredService<ISettingsProvider>();

                #region Exception handling

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

                if (UIOptions.EnableStatusCodePages)
                    app.UseStatusCodePages();

                app.UseMiddleware<ExceptionFilterMiddleware>();

                #endregion

                if (!IsRunningBehindProxy)
                    app.UseHttpsRedirection();

                #region Response compression & minification

                if (UIOptions.Views.EnableResponseMinification || UIOptions.EnableResponseCompression)
                    app.UseWebMarkupMin();

                #endregion

                #region Bundling

                app.UseBundling(new Bundles());

                #endregion

                #region Static files

                var staticFileOptions = new StaticFileOptions();
                if (UIOptions.StaticFiles.EnableResponseCaching)
                    staticFileOptions.OnPrepareResponse = context =>
                    {
                        var headers = context.Context.Response.GetTypedHeaders();
                        headers.CacheControl = new CacheControlHeaderValue { MaxAge = UIOptions.StaticFiles.CacheHeaderMaxAge ?? UIOptions.DefaultCacheHeaderMaxAge };
                    };

                app.UseStaticFiles(staticFileOptions);

                #endregion

                #region Localization

                if (settingsProvider.EnableLocalization())
                {
                    var supportedCultures = settingsProvider.AvailableCultures(out var defaultCulture);

                    app.UseRequestLocalization(new RequestLocalizationOptions
                    {
                        DefaultRequestCulture = new RequestCulture(defaultCulture, defaultCulture),
                        SupportedCultures = supportedCultures,
                        SupportedUICultures = supportedCultures,
                    });
                }

                #endregion

                app.UseRouting();

                #region Security

                app.UseAuthentication();

                app.UseAuthorization();

                #endregion

                #region View caching

                if (UIOptions.Views.EnableResponseCaching)
                    app.UseResponseCaching();

                #endregion

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "AreaDefault",
                        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                    endpoints.MapControllerRoute(
                        name: "Default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");
                });
            }
        }
    }
}
