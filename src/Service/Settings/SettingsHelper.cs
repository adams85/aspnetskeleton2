using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Karambolo.Common;
using WebApp.Common.Settings;
using WebApp.Core.Helpers;
using WebApp.DataAccess.Entities;

using static WebApp.Common.Settings.SettingEnumConstants;

namespace WebApp.Service.Settings
{
    internal static class SettingsHelper
    {
        private static readonly Expression<Func<Setting, SettingData>> s_toDataExpr = entity => new SettingData
        {
            Name = entity.Name,
            Value = entity.Value,
            DefaultValue = entity.DefaultValue,
            MinValue = entity.MinValue,
            MaxValue = entity.MaxValue
        };

        private static readonly Func<Setting, SettingData> s_toData = s_toDataExpr.Compile();

        public static SettingData ToData(this Setting entity) => s_toData(entity);

        public static IQueryable<SettingData> ToData(this IQueryable<Setting> linq) => linq.Select(s_toDataExpr);

        private static readonly Func<string, string?> s_normalizeCulture = value =>
        {
            try { return CultureInfo.GetCultureInfo(value).Name; }
            catch (CultureNotFoundException) { return null; }
        };

        private static readonly Func<string, string?> s_normalizeTheme = Identity<string>.Func;

        private static readonly EmailAddressAttribute s_emailAddressValidator = new EmailAddressAttribute();

        public static bool Validate(this Setting setting, ref string? value)
        {
            if (Enum.TryParse<SettingEnum>(setting.Name, ignoreCase: false, out var settingEnum))
            {
                switch (settingEnum)
                {
                    case SettingEnum.AdminMailAddress:
                    case SettingEnum.NoReplyMailAddress:
                        return value != null && s_emailAddressValidator.IsValid(value);

                    case SettingEnum.AvailableCultures:
                        if (!StringHelper.TryNormalizeCommaSeparatedList(value, out var items, s_normalizeCulture, StringSplitOptions.RemoveEmptyEntries) || items.Count == 0)
                            return false;

                        value = StringHelper.JoinCommaSeparatedList(items);
                        return true;

                    case SettingEnum.AvailableThemes:
                        if (!StringHelper.TryNormalizeCommaSeparatedList(value, out items, s_normalizeTheme, StringSplitOptions.RemoveEmptyEntries) || items.Count == 0)
                            return false;

                        value = StringHelper.JoinCommaSeparatedList(items);
                        return true;

                    case SettingEnum.EnableLocalization:
                    case SettingEnum.EnableRegistration:
                    case SettingEnum.EnableTheming:
                        return bool.TryParse(value, out var _);

                    case SettingEnum.MaxPageSize:
                        var minValue = int.TryParse(setting.MinValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue) ? intValue : MaxPageSizeMinValue;
                        var maxValue = int.TryParse(setting.MaxValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out intValue) ? intValue : MaxPageSizeMaxValue;

                        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out intValue) &&
                            minValue <= intValue && intValue <= maxValue;
                }
            }

            return true;
        }
    }
}
