using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebApp.Api.Infrastructure.Security
{
    public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        public static void ConfigureOptions<TEvents>(CookieAuthenticationOptions options)
        {
            options.EventsType = typeof(TEvents);
        }

        private readonly ICachedUserInfoProvider _cachedUserInfoProvider;

        public CustomCookieAuthenticationEvents(ICachedUserInfoProvider cachedUserInfoProvider)
        {
            _cachedUserInfoProvider = cachedUserInfoProvider ?? throw new ArgumentNullException(nameof(cachedUserInfoProvider));
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var identity = (ClaimsIdentity?)context.Principal.Identity!;
            var userName = identity.Name;

            var userInfo = await _cachedUserInfoProvider.GetCachedUserInfoAsync(userName, registerActivity: true, context.HttpContext.RequestAborted);

            if (userInfo != null && userInfo.LoginAllowed)
            {
                identity.AddClaimsFrom(userInfo);
            }
            else
            {
                await context.HttpContext.SignOutAsync(context.Scheme.Name);
                context.RejectPrincipal();
            }
        }
    }
}
