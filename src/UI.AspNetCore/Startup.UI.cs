using System;
using System.Reflection;
using Karambolo.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Net.Http.Headers;
using WebApp.Api.Infrastructure.Localization;
using WebApp.Api.Infrastructure.Security;
using WebApp.Core.Helpers;
using WebApp.Service.Settings;
using WebApp.UI.Infrastructure;
using WebApp.UI.Infrastructure.DataAnnotations;
using WebApp.UI.Infrastructure.ErrorHandling;
using WebApp.UI.Infrastructure.Filters;
using WebApp.UI.Infrastructure.Hosting;
using WebApp.UI.Infrastructure.Security;
using WebApp.UI.Infrastructure.Theming;
using WebApp.UI.Infrastructure.ViewFeatures;
using WebApp.UI.Models;

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
                // https://github.com/dotnet/aspnetcore/blob/v6.0.3/src/Mvc/Mvc.Localization/src/MvcLocalizationServices.cs#L14
                service.ServiceType == typeof(IStringLocalizerFactory) || service.ServiceType == typeof(IStringLocalizer<>);

            partial void ConfigureMvcPartial(IMvcBuilder builder);

            public override void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton<IAccountManager, AccountManager>();

                services
                    .AddSingleton<IThemeProvider, ThemeProvider>()
                    .AddSingleton<IThemeManager, ThemeManager>();

                #region Response compression

                if (UIOptions.EnableResponseCompression)
                {
                    services.AddResponseCompression();
                    services.Configure<ResponseCompressionOptions>(Configuration.GetSection("Response:Compression"));
                }

                #endregion

                #region Bundling

                if (Program.UsesDesignTimeBundling)
                    services.AddBundling();
                else
                    Bundles.ConfigureServices(services, Configuration, Environment, UIOptions.Bundles);

                #endregion

                #region Security

                services.AddSingleton<ICachedUserInfoProvider>(sp => sp.GetRequiredService<IAccountManager>());

                // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie
                services.AddAuthentication(UIAuthenticationSchemes.Cookie)
                    .AddCookie(UIAuthenticationSchemes.Cookie);

                services.AddSingleton<CustomCookieAuthenticationEvents>();
                services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
                    .Bind(Configuration.GetSection($"{UISecurityOptions.DefaultSectionName}:Authentication"))
                    .Configure(options =>
                        CustomCookieAuthenticationEvents.ConfigureOptions<CustomCookieAuthenticationEvents>(options));

                services.AddAuthorization(options => AnonymousOnlyAttribute.Configure(options));

                services
                    .ReplaceLast(ServiceDescriptor.Singleton<IAuthorizationPolicyProvider, CustomAuthorizationPolicyProvider>())
                    .AddSingleton<IPageAuthorizationHelper, PageAuthorizationHelper>();

                #endregion

                #region View caching

                if (UIOptions.Views.EnableResponseCaching)
                {
                    services.AddResponseCaching();
                    services.Configure<ResponseCachingOptions>(Configuration.GetSection("Response:ViewCaching"));
                }

                #endregion

                #region MVC

                var mvcBuilder = services.AddRazorPages(options => options.Conventions.Add(new GlobalPageApplicationModelConvention()));

                _apiStartup.ConfigureModelBinding(mvcBuilder);
                _apiStartup.ConfigureModelValidation(mvcBuilder);

                services.Configure<MvcViewOptions>(options => options.ClientModelValidatorProviders.Add(new DataAnnotationsClientLocalizationAdjuster()));

                mvcBuilder
                    .ConfigureApplicationPartManager(manager =>
                    {
                        // ApplicationPartManager finds the Api assembly (as it is referenced), so we have to exclude it manually
                        // because we don't want the API controllers to be visible to the UI branch
                        // https://github.com/dotnet/aspnetcore/blob/v6.0.3/src/Mvc/Mvc.Core/src/DependencyInjection/MvcCoreServiceCollectionExtensions.cs#L91
                        manager.ApplicationParts.RemoveAll((part, _) => part is AssemblyPart assemblyPart && assemblyPart.Assembly == typeof(Api.Startup).Assembly);
                    });

                services.ReplaceLast(ServiceDescriptor.Singleton<IHtmlGenerator, CustomHtmlGenerator>());

                mvcBuilder
                    .AddViewLocalization();

                services
                    .ReplaceLast(ServiceDescriptor.Singleton<IHtmlLocalizerFactory, ExtendedHtmlLocalizerFactory>())
                    .ReplaceLast(ServiceDescriptor.Transient<IViewLocalizer, ExtendedViewLocalizer>());

                services.AddSingleton(typeof(RuntimeCompilationAwareValueCache<>));

                if (UIOptions.EnableRazorRuntimeCompilation)
                    mvcBuilder.AddRazorRuntimeCompilation(suppressExplicitNullableWarning: true);

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
                    app.UseExceptionHandler("/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                if (UIOptions.EnableStatusCodePages)
                    app.UseStatusCodePages();

                app.UseMiddleware<ExceptionFilterMiddleware>();

                #endregion

                if (!IsRunningBehindProxy && !Environment.IsDevelopment())
                    app.UseHttpsRedirection();

                #region Response compression

                if (UIOptions.EnableResponseCompression)
                    app.UseResponseCompression();

                #endregion

                #region Bundling

                if (Program.UsesDesignTimeBundling)
                    app.InitializeBundling();
                else
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
                    endpoints.MapRazorPages();
                });
            }

            /// <remarks>
            /// We need a custom <see cref="IPageApplicationModelConvention"/> implementation because <see cref="RazorPagesOptions.Conventions"/> provides no easy way to
            /// apply customizations (e.g. adding some filters) to all pages of all areas currently (see <seealso href="https://github.com/dotnet/aspnetcore/issues/9783">this issue</seealso>).
            /// </remarks>
            private sealed class GlobalPageApplicationModelConvention : IPageApplicationModelConvention
            {
                public void Apply(PageApplicationModel model)
                {
                    model.Filters.Add(new EnsureHandlerPageFilter());

                    Type pageDescriptorProviderType;
                    if ((pageDescriptorProviderType = model.HandlerType).HasInterface(typeof(IPageDescriptorProvider)) ||
                        (pageDescriptorProviderType = model.PageType).HasInterface(typeof(IPageDescriptorProvider)))
                    {
                        var pageDescriptor = PageDescriptor.Get(pageDescriptorProviderType);

                        if (pageDescriptor is IDynamicAuthorizationPolicyProvider)
                        {
                            var policyName = CustomAuthorizationPolicyProvider.AuthorizePagePolicyPrefix + pageDescriptorProviderType.AssemblyQualifiedNameWithoutAssemblyDetails();
                            model.EndpointMetadata.Add(new AuthorizeAttribute(policyName));
                        }
                    }
                }
            }
        }
    }
}
