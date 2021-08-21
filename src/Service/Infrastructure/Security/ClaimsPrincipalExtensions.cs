using System.Globalization;
using WebApp.Service.Users;

namespace System.Security.Claims
{
    public static class ClaimsPrincipalExtensions
    {
        public static void AddClaimsFrom(this ClaimsIdentity identity, CachedUserInfoData userInfo)
        {
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userInfo.UserId.ToString(CultureInfo.InvariantCulture)));
            identity.AddClaim(new Claim(ClaimTypes.Name, userInfo.UserName));
            identity.AddClaim(new Claim(ClaimTypes.Email, userInfo.Email));

            if (!string.IsNullOrEmpty(userInfo.FirstName))
                identity.AddClaim(new Claim(ClaimTypes.GivenName, userInfo.FirstName));

            if (!string.IsNullOrEmpty(userInfo.LastName))
                identity.AddClaim(new Claim(ClaimTypes.Surname, userInfo.LastName));

            if (userInfo.Roles != null)
                for (int i = 0, n = userInfo.Roles.Length; i < n; i++)
                    identity.AddClaim(new Claim(ClaimTypes.Role, userInfo.Roles[i]));

        }
        public static void AddClaimsFrom(this ClaimsPrincipal principal, CachedUserInfoData userInfo) =>
            ((ClaimsIdentity)principal.Identity).AddClaimsFrom(userInfo);

        public static int? GetUserId(this ClaimsPrincipal principal)
        {
            var claimsIdentity = (ClaimsIdentity)principal.Identity;
            if (!claimsIdentity.IsAuthenticated)
                return null;

            string? id = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return id != null ? int.Parse(id, CultureInfo.InvariantCulture) : (int?)null;
        }

        public static string? GetEmail(this ClaimsPrincipal principal)
        {
            var claimsIdentity = (ClaimsIdentity)principal.Identity;
            return claimsIdentity.IsAuthenticated ? claimsIdentity.FindFirst(ClaimTypes.Email)?.Value : null;
        }

        public static string? GetFirstName(this ClaimsPrincipal principal)
        {
            var claimsIdentity = (ClaimsIdentity)principal.Identity;
            return claimsIdentity.IsAuthenticated ? claimsIdentity.FindFirst(ClaimTypes.GivenName)?.Value : null;
        }

        public static string? GetLastName(this ClaimsPrincipal principal)
        {
            var claimsIdentity = (ClaimsIdentity)principal.Identity;
            return claimsIdentity.IsAuthenticated ? claimsIdentity.FindFirst(ClaimTypes.Surname)?.Value : null;
        }
    }
}
