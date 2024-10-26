using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebApp.UI.Infrastructure.Security;

public static class UIAuthenticationSchemes
{
    public const string Cookie = CookieAuthenticationDefaults.AuthenticationScheme;
}
