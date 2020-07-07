using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WebApp.Common.Infrastructure.Localization;

namespace WebApp.Common.Settings
{
    [DataContract]
    public enum SettingEnum
    {
        [Description(SettingEnumTexts.MaxPageSizeDescription)]
        [DefaultValue(500), Range(10, 5000)]
        [EnumMember] MaxPageSize,
    }

    internal static class SettingEnumTexts
    {
        [Localized]
        public const string MaxPageSizeDescription =
            "Global limit on list page size. " +
            "To guard the server against overloading, no query may return more items than the specified value in a single HTTP request. " +
            "The value must be between {1} and {2}.";
    }
}
