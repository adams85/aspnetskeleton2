using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebApp.UI.Infrastructure.Security
{
    public class UICookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly IAccountManager _accountManager;

        public UICookieAuthenticationEvents(IAccountManager accountManager)
        {
            _accountManager = accountManager;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var userPrincipal = context.Principal;

            var userInfo = await _accountManager.GetCachedUserInfo(userPrincipal.Identity.Name!, registerActivity: true,
                cancellationToken: context.HttpContext.RequestAborted);

            if (userInfo == null || !userInfo.LoginAllowed)
            {
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.RejectPrincipal();
            }
            else
                context.Principal.AddClaimsFrom(userInfo);
        }
    }
}
