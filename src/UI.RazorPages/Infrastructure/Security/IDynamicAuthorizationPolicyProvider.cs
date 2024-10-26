using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace WebApp.UI.Infrastructure.Security;

public interface IDynamicAuthorizationPolicyProvider
{
    Task<AuthorizationPolicy?> GetAuthorizationPolicyAsync(HttpContext httpContext);
}
