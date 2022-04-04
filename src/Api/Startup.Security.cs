using Karambolo.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApp.Api.Infrastructure.Security;

namespace WebApp.Api
{
    public partial class Startup
    {
        private void ConfigureSecurityServices(IServiceCollection services)
        {
            services.AddSingleton<IApiSecurityService, ApiSecurityService>();
            services.AddSingleton<ICachedUserInfoProvider>(sp => sp.GetRequiredService<IApiSecurityService>());

            services.AddAuthentication(ApiAuthenticationSchemes.JwtBearer)
                .AddJwtBearer(ApiAuthenticationSchemes.JwtBearer, CachedDelegates.Noop<JwtBearerOptions>.Action)
                .AddCookie(ApiAuthenticationSchemes.Cookie);

            services.AddOptions<JwtBearerOptions>(ApiAuthenticationSchemes.JwtBearer)
                .Configure<IOptions<ApiSecurityOptions>, ILogger<IApiSecurityService>>((options, securityOptions, logger) =>
                    CustomJwtBearerEvents.ConfigureOptions(options, securityOptions, logger));

            services.AddSingleton<CustomCookieAuthenticationEvents>();
            services.AddOptions<CookieAuthenticationOptions>(ApiAuthenticationSchemes.Cookie)
                .Configure(options =>
                    CustomCookieAuthenticationEvents.ConfigureOptions<CustomCookieAuthenticationEvents>(options));
        }

        private void ConfigureSecurity(IApplicationBuilder app)
        {
            // https://garywoodfine.com/asp-net-core-2-2-jwt-authentication-tutorial/
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthorization();
        }
    }
}
