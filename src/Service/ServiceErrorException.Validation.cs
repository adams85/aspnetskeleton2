using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using WebApp.Service.Infrastructure.Validation;

namespace WebApp.Service
{
    public partial class ServiceErrorException : ApplicationException
    {
        private static bool TryGetValidationResultProperty(ValidationResult? validationResult, object key, [MaybeNullWhen(false)] out object value)
        {
            if (validationResult is ExtendedValidationResult extendedValidationResult && extendedValidationResult.Properties.TryGetValue(key, out value))
                return true;

            value = default;
            return false;
        }

        internal static ServiceErrorException From(ValidationException exception)
        {
            var memberPath = exception.GetMemberPath(exception.ValidationResult?.MemberNames?.FirstOrDefault());

            var (errorCode, args) = exception.ValidationAttribute switch
            {
                RequiredAttribute _ => (ServiceErrorCode.ParamNotSpecified, new[] { memberPath }),
                PasswordAttribute _ when TryGetValidationResultProperty(exception.ValidationResult, PasswordValidator.PasswordRequirementsPropertyKey, out var passwordRequirements) =>
                    (ServiceErrorCode.ParamNotValid, new object[] { memberPath, passwordRequirements }),
                _ => (ServiceErrorCode.ParamNotValid, new[] { memberPath })
            };

            return new ServiceErrorException(errorCode, args);
        }
    }
}
