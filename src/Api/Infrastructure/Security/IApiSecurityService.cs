using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace WebApp.Api.Infrastructure.Security
{
    public interface IApiSecurityService : ICachedUserInfoProvider
    {
        void ConfigureCookieAuthentication(CookieAuthenticationOptions options);
        void ConfigureJwtBearerAuthentication(JwtBearerOptions options);

        Task<bool> TryIssueJwtTokenAsync(NetworkCredential credentials, HttpContext httpContext);
    }
}
