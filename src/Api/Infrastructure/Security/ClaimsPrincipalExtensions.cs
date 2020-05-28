using System.Globalization;

namespace System.Security.Claims
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            var claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;
            if (!claimsIdentity.IsAuthenticated)
                return null;

            string? id = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return id != null ? int.Parse(id, CultureInfo.InvariantCulture) : (int?)null;
        }

        public static string? GetEmail(this ClaimsPrincipal claimsPrincipal)
        {
            var claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;
            return claimsIdentity.IsAuthenticated ? claimsIdentity.FindFirst(ClaimTypes.Email)?.Value : null;
        }

        public static string? GetFirstName(this ClaimsPrincipal claimsPrincipal)
        {
            var claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;
            return claimsIdentity.IsAuthenticated ? claimsIdentity.FindFirst(ClaimTypes.GivenName)?.Value : null;
        }

        public static string? GetLastName(this ClaimsPrincipal claimsPrincipal)
        {
            var claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;
            return claimsIdentity.IsAuthenticated ? claimsIdentity.FindFirst(ClaimTypes.Surname)?.Value : null;
        }
    }
}
