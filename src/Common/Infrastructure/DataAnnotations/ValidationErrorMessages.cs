using WebApp.Common.Infrastructure.Localization;

namespace System.ComponentModel.DataAnnotations
{
    // based on: https://github.com/dotnet/corefx/blob/v3.1.6/src/System.ComponentModel.Annotations/src/Resources/Strings.resx
    public static class ValidationErrorMessages
    {
        #region Built-in validation attributes

        [Localized] private const string CompareAttribute_DefaultErrorMessage = "'{0}' and '{1}' do not match.";

        [Localized] private const string FileExtensionsAttribute_DefaultErrorMessage = "The {0} field only accepts files with the following extensions: {1}";

        [Localized] private const string MaxLengthAttribute_DefaultErrorMessage = "The field {0} must be a string or array type with a maximum length of '{1}'.";

        [Localized] private const string MinLengthAttribute_DefaultErrorMessage = "The field {0} must be a string or array type with a minimum length of '{1}'.";

        [Localized] private const string RangeAttribute_DefaultErrorMessage = "The field {0} must be between {1} and {2}.";

        [Localized] private const string RegularExpressionAttribute_DefaultErrorMessage = "The field {0} must match the regular expression '{1}'.";

        [Localized] private const string RequiredAttribute_DefaultErrorMessage = "The {0} field is required.";

        [Localized] private const string StringLengthAttribute_DefaultErrorMessage = "The field {0} must be a string with a maximum length of {1}.";
        [Localized] private const string StringLengthAttribute_IncludingMinimumErrorMessage = "The field {0} must be a string with a minimum length of {2} and a maximum length of {1}.";

        [Localized] private const string ValidationAttribute_DefaultErrorMessage = "The field {0} is invalid.";

        #endregion

        #region Custom validation attributes

        [Localized] internal const string ItemsRequiredAttribute_DefaultErrorMessage = "The field {0} must be a collection containing no unset items.";

        [Localized] internal const string PasswordAttribute_DefaultErrorMessage = "The field {0} must meet password complexity requirements.";

        #endregion

        public static string GetDefaultErrorMessage(this ValidationAttribute attribute) => attribute switch
        {
            CompareAttribute _ => CompareAttribute_DefaultErrorMessage,
            FileExtensionsAttribute _ => FileExtensionsAttribute_DefaultErrorMessage,
            MaxLengthAttribute _ => MaxLengthAttribute_DefaultErrorMessage,
            MinLengthAttribute _ => MinLengthAttribute_DefaultErrorMessage,
            RangeAttribute _ => RangeAttribute_DefaultErrorMessage,
            RegularExpressionAttribute _ => RegularExpressionAttribute_DefaultErrorMessage,
            RequiredAttribute _ => RequiredAttribute_DefaultErrorMessage,
            StringLengthAttribute stringLengthAttribute =>
                stringLengthAttribute.MinimumLength != 0 ?
                StringLengthAttribute_IncludingMinimumErrorMessage :
                StringLengthAttribute_DefaultErrorMessage,

            ItemsRequiredAttribute _ => ItemsRequiredAttribute_DefaultErrorMessage,
            PasswordAttribute _ => PasswordAttribute_DefaultErrorMessage,

            _ => ValidationAttribute_DefaultErrorMessage,
        };
    }
}
