﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Net.Http.Headers;
using WebApp.Api.Infrastructure.Localization;
using WebApp.Core.Helpers;
using WebApp.Service.Settings;
using WebApp.UI.Areas.Dashboard;
using WebApp.UI.Infrastructure.DataAnnotations;
using WebApp.UI.Infrastructure.Hosting;
using WebApp.UI.Infrastructure.Navigation;
using WebApp.UI.Infrastructure.Security;
using WebApp.UI.Infrastructure.Theming;
using WebApp.UI.Infrastructure.ViewFeatures;
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

            protected override bool ShouldResolveFromRoot(ServiceDescriptor service) =>
                // AddDataAnnotationsLocalization, AddViewLocalization calls AddLocalization under the hood, that is, it adds base localization services,
                // but those are already registered in the root container and we need those shared instances
                // https://github.com/dotnet/aspnetcore/blob/v3.1.6/src/Mvc/Mvc.Localization/src/MvcLocalizationServices.cs#L14
                service.ServiceType == typeof(IStringLocalizerFactory) || service.ServiceType == typeof(IStringLocalizer<>);

            partial void ConfigureMvcPartial(IMvcBuilder builder);

            public override void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton<IPageCatalog>(sp => new PageCatalog(new IPageCollectionProvider[]
                {
                    new Pages(),
                    new DashboardPages(),
                }));

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

                var mvcBuilder = services.AddControllersWithViews();

                _apiStartup.ConfigureDataAnnotationServices(mvcBuilder);

                services.Configure<MvcViewOptions>(options => options.ClientModelValidatorProviders.Add(new DataAnnotationsClientLocalizationAdjuster()));

                mvcBuilder
                    .ConfigureApplicationPartManager(manager =>
                    {
                        // ApplicationPartManager finds the Api assembly (as it is referenced), so we have to exclude it manually
                        // because we don't want the API controllers to be visible to the UI branch
                        // https://github.com/dotnet/aspnetcore/blob/v3.1.6/src/Mvc/Mvc.Core/src/DependencyInjection/MvcCoreServiceCollectionExtensions.cs#L81
                        manager.ApplicationParts.RemoveAll((part, _) => part is AssemblyPart assemblyPart && assemblyPart.Assembly == typeof(Api.Startup).Assembly);
                    });

                services.ReplaceLast(ServiceDescriptor.Singleton<IHtmlGenerator, CustomHtmlGenerator>());

                mvcBuilder
                    .AddViewLocalization();

                services
                    .ReplaceLast(ServiceDescriptor.Singleton<IHtmlLocalizerFactory, ExtendedHtmlLocalizerFactory>())
                    .ReplaceLast(ServiceDescriptor.Singleton<IViewLocalizer, ExtendedViewLocalizer>());

                if (UIOptions.EnableRazorRuntimeCompilation)
                    mvcBuilder.AddRazorRuntimeCompilation();

                #endregion

                #region Global Razor helpers

                services.AddSingleton<IGlobalRazorHelpersFactory, GlobalRazorHelpersFactory>();
                services.AddTransient(typeof(IGlobalRazorHelpers<>), typeof(GlobalRazorHelpers<>));

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
                    endpoints.MapControllers();
                });
            }
        }
    }
}
