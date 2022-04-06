using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WebApp.Common.Infrastructure.Localization;
using WebApp.Common.Infrastructure.Validation;

namespace WebApp.Service.Infrastructure.Validation
{
    public sealed class PasswordValidator : IValidator<PasswordAttribute>
    {
        public static readonly object PasswordRequirementsPropertyKey = new object();

        private static PasswordRequirementsData? GetPasswordRequirements(PasswordOptions passwordOptions) => new PasswordRequirementsData()
        {
            RequiredLength = passwordOptions.RequiredLength,
            RequiredUniqueChars = passwordOptions.RequiredUniqueChars,
            RequireNonAlphanumeric = passwordOptions.RequireNonAlphanumeric,
            RequireLowercase = passwordOptions.RequireLowercase,
            RequireUppercase = passwordOptions.RequireUppercase,
            RequireDigit = passwordOptions.RequireDigit
        };

        private readonly PasswordRequirementsData? _passwordRequirements;

        public PasswordValidator(IOptions<PasswordOptions>? passwordOptions)
        {
            var passwordOptionsValue = passwordOptions?.Value;
            _passwordRequirements = passwordOptionsValue != null ? GetPasswordRequirements(passwordOptionsValue) : null;
        }

        private bool IsValidPassword(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < _passwordRequirements!.RequiredLength)
                return false;

            if (_passwordRequirements.RequireNonAlphanumeric && value.All(c => char.IsDigit(c) || char.IsLetter(c)))
                return false;

            if (_passwordRequirements.RequireDigit && !value.Any(char.IsDigit))
                return false;

            if (_passwordRequirements.RequireLowercase && !value.Any(char.IsLower))
                return false;

            if (_passwordRequirements.RequireUppercase && !value.Any(char.IsUpper))
                return false;

            if (_passwordRequirements.RequiredUniqueChars >= 1 && value.Distinct().Count() < _passwordRequirements.RequiredUniqueChars)
                return false;

            return true;
        }

        public string FormatErrorMessage(string localizedName, ITextLocalizer textLocalizer, PasswordAttribute validationAttribute) =>
            validationAttribute.FormatErrorMessage(localizedName, textLocalizer);

        public ValidationResult? IsValid(object? value, ValidationContext validationContext, PasswordAttribute validationAttribute)
        {
            if (value is not string stringValue)
                return ValidationResult.Success;

            if (_passwordRequirements == null)
                return
                    validationAttribute.IgnoreIfServiceUnavailable ?
                    ValidationResult.Success :
                    throw new InvalidOperationException($"Password options {typeof(IOptions<PasswordOptions>)} has not been registered or configured.");

            if (IsValidPassword(stringValue))
                return ValidationResult.Success;

            var result = new ExtendedValidationResult(validationAttribute, validationContext, new[] { validationContext.MemberName! });

            if (_passwordRequirements != null)
                result.Properties[PasswordRequirementsPropertyKey] = _passwordRequirements;

            return result;
        }
    }
}
