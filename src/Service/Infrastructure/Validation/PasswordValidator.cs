using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Karambolo.Common.Localization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using WebApp.Common.Infrastructure.Localization;
using WebApp.Common.Infrastructure.Validation;

namespace WebApp.Service.Infrastructure.Validation
{
    public sealed class PasswordValidator : IValidator<PasswordAttribute>
    {
        private readonly PasswordOptions? _passwordOptions;

        public PasswordValidator(IStringLocalizer<PasswordValidator> stringLocalizer, IOptions<PasswordOptions>? passwordOptions)
        {
            _passwordOptions = passwordOptions?.Value;
            T = stringLocalizer;
        }

        private IStringLocalizer<PasswordValidator> T { get; }

        private bool IsValidPassword(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < _passwordOptions!.RequiredLength)
                return false;

            if (_passwordOptions.RequireNonAlphanumeric && value.All(c => char.IsDigit(c) || char.IsLetter(c)))
                return false;

            if (_passwordOptions.RequireDigit && !value.Any(char.IsDigit))
                return false;

            if (_passwordOptions.RequireLowercase && !value.Any(char.IsLower))
                return false;

            if (_passwordOptions.RequireUppercase && !value.Any(char.IsUpper))
                return false;

            if (_passwordOptions.RequiredUniqueChars >= 1 && value.Distinct().Count() < _passwordOptions.RequiredUniqueChars)
                return false;

            return true;
        }

        public string FormatErrorMessage(string localizedName, ITextLocalizer textLocalizer, PasswordAttribute validationAttribute)
        {
            var errorMessage = validationAttribute.FormatErrorMessage(localizedName, textLocalizer);

            if (!validationAttribute.IncludeComplexityRequirementsInErrorMessage)
                return errorMessage;

            var sb = new StringBuilder(errorMessage);

            sb.AppendLine();
            sb.Append(T["Passwords must be at least {0} character long.", Plural.From("Passwords must be at least {0} characters long.", Math.Max(1, _passwordOptions!.RequiredLength))]);

            if (_passwordOptions.RequireNonAlphanumeric || _passwordOptions.RequireDigit || _passwordOptions.RequireLowercase || _passwordOptions.RequireUppercase)
            {
                sb.AppendLine();
                sb.Append(T["Must contain characters from the following categories:"]).Append(' ');

                if (_passwordOptions.RequireNonAlphanumeric)
                    sb.Append(T["non-alphabetic characters (!, $, #, etc.)"]).Append(", ");

                if (_passwordOptions.RequireDigit)
                    sb.Append(T["base 10 digits (0-9)"]).Append(", ");

                if (_passwordOptions.RequireLowercase)
                    sb.Append(T["English lowercase characters (a-z)"]).Append(", ");

                if (_passwordOptions.RequireUppercase)
                    sb.Append(T["English uppercase characters (A-Z)"]).Append(", ");

                sb.Remove(sb.Length - 2, 2).Append('.');
            }

            if (_passwordOptions.RequiredUniqueChars > 0)
            {
                sb.AppendLine();
                sb.Append(T["Must contain at least {0} different character.", Plural.From("Must contain at least {0} different characters.", _passwordOptions.RequiredUniqueChars)]);
            }

            return sb.ToString();
        }

        public ValidationResult IsValid(object? value, ValidationContext validationContext, PasswordAttribute validationAttribute)
        {
            if (!(value is string stringValue))
                return ValidationResult.Success;

            if (_passwordOptions == null)
                return
                    validationAttribute.IgnoreIfServiceUnavailable ?
                    ValidationResult.Success :
                    throw new InvalidOperationException($"Password options {typeof(IOptions<PasswordOptions>)} has not been registered or configured.");

            if (IsValidPassword(stringValue))
                return ValidationResult.Success;

            return new ExtendedValidationResult(validationAttribute, validationContext, new[] { validationContext.MemberName });
        }
    }
}
