using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebApp.Api.Infrastructure.Security
{
    public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly ICachedUserInfoProvider _cachedUserInfoProvider;
        private readonly string _scheme;

        public CustomCookieAuthenticationEvents(ICachedUserInfoProvider cachedUserInfoProvider, string scheme = CookieAuthenticationDefaults.AuthenticationScheme)
        {
            _cachedUserInfoProvider = cachedUserInfoProvider ?? throw new ArgumentNullException(nameof(cachedUserInfoProvider));
            _scheme = scheme;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var identity = (ClaimsIdentity?)context.Principal.Identity!;
            var userName = identity.Name;

            var userInfo = await _cachedUserInfoProvider.GetCachedUserInfo(userName, registerActivity: true, context.HttpContext.RequestAborted);

            if (userInfo != null && userInfo.LoginAllowed)
            {
                identity.AddClaimsFrom(userInfo);
            }
            else
            {
                await context.HttpContext.SignOutAsync(_scheme);
                context.RejectPrincipal();
            }
        }
    }
}
