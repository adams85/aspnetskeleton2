using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebApp.UI.Infrastructure.Security
{
    public interface IPageAuthorizationHelper
    {
        Task<bool> CheckAccessAllowedAsync(HttpContext httpContext, string pageRoute, string areaName);
    }
}
