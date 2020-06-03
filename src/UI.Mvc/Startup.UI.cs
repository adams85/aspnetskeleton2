using System;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                        manager.ApplicationParts.RemoveAll((part, _) => part is AssemblyPart assemblyPart && assemblyPart.Assembly == typeof(Api.Startup).Assembly);
                    });

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
