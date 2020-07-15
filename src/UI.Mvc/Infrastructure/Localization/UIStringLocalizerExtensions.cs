using System;
using System.Text;
using Karambolo.Common.Localization;
using Microsoft.Extensions.Localization;
using WebApp.Service.Infrastructure.Validation;

namespace WebApp.UI.Infrastructure.Localization
{
    public static class UIStringLocalizerExtensions
    {
        public static string LocalizePasswordRequirements(
#pragma warning disable IDE1006 // Naming Styles
            this IStringLocalizer T,
#pragma warning restore IDE1006 // Naming Styles
            PasswordRequirementsData? passwordRequirements)
        {
            var errorMessage = T["The password does not meet the complexity requirements."];

            if (passwordRequirements == null)
                return errorMessage;

            var sb = new StringBuilder(errorMessage);

            sb.Append(' ');
            sb.Append(T["Passwords must be at least {0} character long.", Plural.From("Passwords must be at least {0} characters long.", Math.Max(1, passwordRequirements!.RequiredLength))]);

            if (passwordRequirements.RequireNonAlphanumeric || passwordRequirements.RequireDigit || passwordRequirements.RequireLowercase || passwordRequirements.RequireUppercase)
            {
                sb.Append(' ');
                sb.Append(T["Must contain at least one character from the following category(s):"]).Append(' ');

                if (passwordRequirements.RequireNonAlphanumeric)
                    sb.Append(T["non-alphabetic characters (!, $, #, etc.)"]).Append(", ");

                if (passwordRequirements.RequireDigit)
                    sb.Append(T["base 10 digits (0-9)"]).Append(", ");

                if (passwordRequirements.RequireLowercase)
                    sb.Append(T["English lowercase characters (a-z)"]).Append(", ");

                if (passwordRequirements.RequireUppercase)
                    sb.Append(T["English uppercase characters (A-Z)"]).Append(", ");

                sb.Remove(sb.Length - 2, 2).Append('.');
            }

            if (passwordRequirements.RequiredUniqueChars > 1)
            {
                sb.Append(' ');
                sb.Append(T["Must contain at least {0} different character.", Plural.From("Must contain at least {0} different characters.", passwordRequirements.RequiredUniqueChars)]);
            }

            return sb.ToString();
        }
    }
}
