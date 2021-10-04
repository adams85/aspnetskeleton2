using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace WebApp.Api.Infrastructure.Security
{
    public static class ApiAuthenticationSchemes
    {
        public const string JwtBearer = JwtBearerDefaults.AuthenticationScheme;
        public const string Cookie = CookieAuthenticationDefaults.AuthenticationScheme;

        // order matters: when multiple authentication schemes succeed, the last scheme will provide the primary identity in the resulting user principal
        public const string CookieAndJwtBearer = Cookie + "," + JwtBearer; // order
    }
}
