using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebApp.Api.Infrastructure.Security
{
    public interface IApiSecurityService : ICachedUserInfoProvider
    {
        Task<bool> TryIssueJwtTokenAsync(NetworkCredential credentials, HttpContext httpContext);
    }
}
