using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using WebApp.UI.Models;

namespace WebApp.UI.Infrastructure.Security;

public sealed class CustomAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public const string AuthorizePagePolicyPrefix = "AuthorizePage_";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomAuthorizationPolicyProvider(IHttpContextAccessor httpContextAccessor, IOptions<AuthorizationOptions> authorizationOptions)
        : base(authorizationOptions)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(AuthorizePagePolicyPrefix, StringComparison.Ordinal))
        {
            var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context is not available.");

            var pageDescriptorProviderType = Type.GetType(policyName.Substring(AuthorizePagePolicyPrefix.Length), throwOnError: true)!;
            var pageDescriptor = PageDescriptor.Get(pageDescriptorProviderType);

            return ((IDynamicAuthorizationPolicyProvider)pageDescriptor).GetAuthorizationPolicyAsync(httpContext);
        }

        return base.GetPolicyAsync(policyName);
    }
}
