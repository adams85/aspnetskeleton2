using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer()
                .AddCookie(options => options.ForwardChallenge = JwtBearerDefaults.AuthenticationScheme);

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IApiSecurityService>((options, securityService) => securityService.ConfigureJwtBearer(options));

            services.AddAuthorization(ConfigureAuthorization);
        }

        private void ConfigureAuthorization(AuthorizationOptions options)
        {
            // https://stackoverflow.com/questions/43800763/using-multiple-authentication-schemes-in-asp-net-core
            options.AddPolicy(ApiSecurityService.ApiAuthorizationPolicy, builder =>
            {
                builder.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme);
                builder.RequireAuthenticatedUser();
            });
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
