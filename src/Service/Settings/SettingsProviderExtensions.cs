using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using WebApp.Common.Settings;
using WebApp.Core.Helpers;
using static WebApp.Common.Settings.SettingEnumConstants;

namespace WebApp.Service.Settings;

public static class SettingsProviderExtensions
{
    [return: MaybeNull]
    public static T Get<T>(this ISettingsProvider provider, SettingEnum setting)
    {
        Type? type = typeof(T);

        if (type.IsValueType)
        {
            type = Nullable.GetUnderlyingType(typeof(T));
            if (type == null)
                throw new ArgumentException("Type parameter must be a type accepting null values.", nameof(T));
        }

        var value = provider[setting.ToString()];
        return value != null ? (T)Convert.ChangeType(value, type, CultureInfo.InvariantCulture) : default;
    }

    public static string AdminMailAddress(this ISettingsProvider provider) =>
        provider.Get<string?>(SettingEnum.AdminMailAddress) ?? AdminMailAddressDefaultValue;

    public static CultureInfo[] AvailableCultures(this ISettingsProvider provider) => provider.AvailableCultures(out _);

    public static CultureInfo[] AvailableCultures(this ISettingsProvider provider, out CultureInfo defaultCulture)
    {
        var value = provider[SettingEnum.AvailableCultures.ToString()] ?? AvailableCulturesDefaultValue;
        var cultures = StringHelper.SplitCommaSeparatedList(value).Select(CultureInfo.GetCultureInfo).ToArray();
        defaultCulture = cultures[0];
        return cultures;
    }

    public static string[] AvailableThemes(this ISettingsProvider provider) => provider.AvailableThemes(out _);

    public static string[] AvailableThemes(this ISettingsProvider provider, out string defaultTheme)
    {
        var value = provider[SettingEnum.AvailableThemes.ToString()] ?? AvailableThemesDefaultValue;
        var themes = StringHelper.SplitCommaSeparatedList(value).ToArray();
        defaultTheme = themes[0];
        return themes;
    }

    public static bool EnableLocalization(this ISettingsProvider provider) =>
        provider.Get<bool?>(SettingEnum.EnableLocalization) ?? EnableLocalizationDefaultValue;

    public static bool EnableRegistration(this ISettingsProvider provider) =>
        provider.Get<bool?>(SettingEnum.EnableRegistration) ?? EnableRegistrationDefaultValue;

    public static bool EnableSwagger(this ISettingsProvider provider) =>
        provider.Get<bool?>(SettingEnum.EnableSwagger) ?? EnableSwaggerDefaultValue;

    public static bool EnableTheming(this ISettingsProvider provider) =>
        provider.Get<bool?>(SettingEnum.EnableTheming) ?? EnableThemingDefaultValue;

    public static int MaxPageSize(this ISettingsProvider provider) =>
        provider.Get<int?>(SettingEnum.MaxPageSize) ?? MaxPageSizeDefaultValue;

    public static string NoReplyMailAddress(this ISettingsProvider provider) =>
        provider.Get<string?>(SettingEnum.NoReplyMailAddress) ?? NoReplyMailAddressDefaultValue;
}
