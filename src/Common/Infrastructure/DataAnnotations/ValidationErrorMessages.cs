using WebApp.Common.Infrastructure.Localization;

namespace System.ComponentModel.DataAnnotations
{
    // based on: https://github.com/dotnet/runtime/blob/main/src/libraries/System.ComponentModel.Annotations/src/Resources/Strings.resx
    public static class ValidationErrorMessages
    {
        #region Built-in validation attributes

        [Localized] public const string CompareAttribute_DefaultErrorMessage = "'{0}' and '{1}' do not match.";

        [Localized] public const string CreditCardAttribute_DefaultErrorMessage = "The {0} field is not a valid credit card number.";

        [Localized] public const string EmailAddressAttribute_DefaultErrorMessage = "The {0} field is not a valid e-mail address.";

        [Localized] public const string FileExtensionsAttribute_DefaultErrorMessage = "The {0} field only accepts files with the following extensions: {1}";

        [Localized] public const string MaxLengthAttribute_DefaultErrorMessage = "The field {0} must be a string or array type with a maximum length of '{1}'.";

        [Localized] public const string MinLengthAttribute_DefaultErrorMessage = "The field {0} must be a string or array type with a minimum length of '{1}'.";

        [Localized] public const string PhoneAttribute_DefaultErrorMessage = "The {0} field is not a valid phone number.";

        [Localized] public const string RangeAttribute_DefaultErrorMessage = "The field {0} must be between {1} and {2}.";

        [Localized] public const string RegularExpressionAttribute_DefaultErrorMessage = "The field {0} must match the regular expression '{1}'.";

        [Localized] public const string RequiredAttribute_DefaultErrorMessage = "The {0} field is required.";

        [Localized] public const string StringLengthAttribute_DefaultErrorMessage = "The field {0} must be a string with a maximum length of {1}.";
        [Localized] public const string StringLengthAttribute_IncludingMinimumErrorMessage = "The field {0} must be a string with a minimum length of {2} and a maximum length of {1}.";

        [Localized] public const string UrlAttribute_IncludingMinimumErrorMessage = "The {0} field is not a valid fully-qualified http, https, or ftp URL.";

        [Localized] public const string ValidationAttribute_DefaultErrorMessage = "The field {0} is invalid.";

        #endregion

        #region Custom validation attributes

        [Localized] public const string ItemsRequiredAttribute_DefaultErrorMessage = "The field {0} must be a collection containing no unset items.";

        [Localized] public const string PasswordAttribute_DefaultErrorMessage = "The field {0} must meet password complexity requirements.";

        #endregion

        public static string GetDefaultErrorMessage(this ValidationAttribute attribute) => attribute switch
        {
            CompareAttribute _ => CompareAttribute_DefaultErrorMessage,
            CreditCardAttribute _ => CreditCardAttribute_DefaultErrorMessage,
            EmailAddressAttribute _ => EmailAddressAttribute_DefaultErrorMessage,
            FileExtensionsAttribute _ => FileExtensionsAttribute_DefaultErrorMessage,
            MaxLengthAttribute _ => MaxLengthAttribute_DefaultErrorMessage,
            MinLengthAttribute _ => MinLengthAttribute_DefaultErrorMessage,
            PhoneAttribute _ => PhoneAttribute_DefaultErrorMessage,
            RangeAttribute _ => RangeAttribute_DefaultErrorMessage,
            RegularExpressionAttribute _ => RegularExpressionAttribute_DefaultErrorMessage,
            RequiredAttribute _ => RequiredAttribute_DefaultErrorMessage,
            StringLengthAttribute stringLengthAttribute =>
                stringLengthAttribute.MinimumLength != 0 ?
                StringLengthAttribute_IncludingMinimumErrorMessage :
                StringLengthAttribute_DefaultErrorMessage,
            UrlAttribute _ => UrlAttribute_IncludingMinimumErrorMessage,

            ItemsRequiredAttribute _ => ItemsRequiredAttribute_DefaultErrorMessage,
            PasswordAttribute _ => PasswordAttribute_DefaultErrorMessage,

            _ => ValidationAttribute_DefaultErrorMessage,
        };
    }
}
