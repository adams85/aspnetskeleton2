using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Api.Infrastructure.Security;

namespace WebApp.Api
{
    public partial class Startup
    {
        private void ConfigureSecurityServices(IServiceCollection services)
        {
            services.AddSingleton<IApiSecurityService, ApiSecurityService>();

            services.AddAuthentication(ApiAuthenticationSchemes.JwtBearer)
                .AddJwtBearer()
                .AddCookie();

            services.AddOptions<JwtBearerOptions>(ApiAuthenticationSchemes.JwtBearer)
                .Configure<IApiSecurityService>((options, securityService) => securityService.ConfigureJwtBearerAuthentication(options));

            services.AddOptions<CookieAuthenticationOptions>(ApiAuthenticationSchemes.Cookie)
                .Configure<IApiSecurityService>((options, securityService) => securityService.ConfigureCookieAuthentication(options));
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
