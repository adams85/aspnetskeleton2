using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace WebApp.Api.Infrastructure.Security
{
    public interface ISecurityService
    {
        void ConfigureJwtBearer(JwtBearerOptions options);

        Task<bool> TryIssueJwtTokenAsync(NetworkCredential credentials, HttpContext httpContext);
    }
}
