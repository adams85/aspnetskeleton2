using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WebApp.Common.Infrastructure.Localization;

using static WebApp.Common.Settings.SettingEnumConstants;

namespace WebApp.Common.Settings
{
    [DataContract]
    public enum SettingEnum
    {
        [Description(AdminMailAddressDescription), DefaultValue(AdminMailAddressDefaultValue)]
        [EnumMember] AdminMailAddress,

        [Description(AvailableCulturesDescription), DefaultValue(AvailableCulturesDefaultValue)]
        [EnumMember] AvailableCultures,

        [Description(AvailableThemesDescription), DefaultValue(AvailableThemesDefaultValue)]
        [EnumMember] AvailableThemes,

        [Description(EnableLocalizationDescription), DefaultValue(EnableLocalizationDefaultValue)]
        [EnumMember] EnableLocalization,

        [Description(EnableRegistrationDescription), DefaultValue(EnableRegistrationDefaultValue)]
        [EnumMember] EnableRegistration,

        [Description(EnableSwaggerDescription), DefaultValue(EnableSwaggerDefaultValue)]
        [EnumMember] EnableSwagger,

        [Description(EnableThemingDescription), DefaultValue(EnableThemingDefaultValue)]
        [EnumMember] EnableTheming,

        [Description(MaxPageSizeDescription), DefaultValue(MaxPageSizeDefaultValue), Range(MaxPageSizeMinValue, MaxPageSizeMaxValue)]
        [EnumMember] MaxPageSize,

        [Description(NoReplyMailAddressDescription), DefaultValue(NoReplyMailAddressDefaultValue)]
        [EnumMember] NoReplyMailAddress,
    }

    public static class SettingEnumConstants
    {
        [Localized]
        internal const string AdminMailAddressDescription = "The e-mail address of the system administrator.";
        public const string AdminMailAddressDefaultValue = "admin@example.com";

        [Localized]
        internal const string AvailableCulturesDescription = "A comma-separated list of cultures which are available for users to choose from. " +
            "The first item in the list used as the default culture. For changes to take effect, the application needs to be restarted.";
        public const string AvailableCulturesDefaultValue = "en-US";

        [Localized]
        internal const string AvailableThemesDescription = "A comma-separated list of themes which are available for users to choose from. " +
            "The first item in the list used as the default theme. For changes to take effect, the application needs to be restarted.";
        public const string AvailableThemesDefaultValue = "Default";

        [Localized]
        internal const string EnableLocalizationDescription = "Specifies whether translation of the application's resources is enabled. " +
            "For changes to take effect, the application needs to be restarted.";
        public const bool EnableLocalizationDefaultValue = true;

        [Localized]
        internal const string EnableRegistrationDescription = "Specifies whether guest users are allowed to create accounts in the application.";
        public const bool EnableRegistrationDefaultValue = true;

        [Localized]
        internal const string EnableSwaggerDescription = "Specifies whether exposing the Swagger (OpenAPI) documentation of the application's web API is enabled.";
        public const bool EnableSwaggerDefaultValue = true;

        [Localized]
        internal const string EnableThemingDescription = "Specifies whether changing the appearance of the user interface is enabled. " +
            "For changes to take effect, the application needs to be restarted.";
        public const bool EnableThemingDefaultValue = true;

        [Localized]
        internal const string MaxPageSizeDescription = "Global limit on list page size. " +
            "To guard the server against overloading, no query may return more items than the specified value in a single HTTP request. " +
            "The value must be between {1} and {2}.";
        public const int MaxPageSizeDefaultValue = 500;
        public const int MaxPageSizeMinValue = 10;
        public const int MaxPageSizeMaxValue = 5000;

        [Localized]
        internal const string NoReplyMailAddressDescription = "The e-mail address to use as sender address for automated mails.";
        public const string NoReplyMailAddressDefaultValue = "no-reply@example.com";
    }
}
