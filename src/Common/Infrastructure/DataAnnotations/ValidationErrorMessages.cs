using WebApp.Common.Infrastructure.Localization;

namespace System.ComponentModel.DataAnnotations;

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
        CompareAttribute => CompareAttribute_DefaultErrorMessage,
        CreditCardAttribute => CreditCardAttribute_DefaultErrorMessage,
        EmailAddressAttribute => EmailAddressAttribute_DefaultErrorMessage,
        FileExtensionsAttribute => FileExtensionsAttribute_DefaultErrorMessage,
        MaxLengthAttribute => MaxLengthAttribute_DefaultErrorMessage,
        MinLengthAttribute => MinLengthAttribute_DefaultErrorMessage,
        PhoneAttribute => PhoneAttribute_DefaultErrorMessage,
        RangeAttribute => RangeAttribute_DefaultErrorMessage,
        RegularExpressionAttribute => RegularExpressionAttribute_DefaultErrorMessage,
        RequiredAttribute => RequiredAttribute_DefaultErrorMessage,
        StringLengthAttribute stringLengthAttribute => stringLengthAttribute.MinimumLength != 0
            ? StringLengthAttribute_IncludingMinimumErrorMessage
            : StringLengthAttribute_DefaultErrorMessage,
        UrlAttribute => UrlAttribute_IncludingMinimumErrorMessage,

        ItemsRequiredAttribute => ItemsRequiredAttribute_DefaultErrorMessage,
        PasswordAttribute => PasswordAttribute_DefaultErrorMessage,

        _ => ValidationAttribute_DefaultErrorMessage,
    };
}
