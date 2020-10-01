using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Karambolo.Common;
using LinqKit;
using Microsoft.Extensions.Localization;
using WebApp.Common.Settings;
using WebApp.Core.Helpers;
using WebApp.DataAccess.Entities;

using static WebApp.Common.Settings.SettingEnumConstants;

namespace WebApp.Service.Settings
{
    internal static class SettingsHelper
    {
        private static readonly Expression<Func<Setting, string?, SettingData>> s_toDataExpr = (entity, description) => new SettingData
        {
            Name = entity.Name,
            Value = entity.Value,
            DefaultValue = entity.DefaultValue,
            MinValue = entity.MinValue,
            MaxValue = entity.MaxValue,
            Description = description
        };

        private static readonly Func<Setting, string?, SettingData> s_toData = s_toDataExpr.Compile();

        private static readonly Expression<Func<string, string?>> s_fallbackNameToDescriptionMapperExpr = _ => null;
        private static readonly Func<string, string?> s_fallbackNameToDescriptionMapper = s_fallbackNameToDescriptionMapperExpr.Compile();

        public static SettingData ToData(this Setting entity, Func<string, string?>? nameToDescriptionMapper = null)
        {
            nameToDescriptionMapper ??= s_fallbackNameToDescriptionMapper;
            return s_toData(entity, nameToDescriptionMapper(entity.Name));
        }

        public static IQueryable<SettingData> ToData(this IQueryable<Setting> linq, Expression<Func<string, string?>>? nameToDescriptionMapper = null)
        {
            nameToDescriptionMapper ??= s_fallbackNameToDescriptionMapperExpr;
            return linq.AsExpandable().Select(entity => s_toDataExpr.Invoke(entity, nameToDescriptionMapper.Invoke(entity.Name)));
        }

        #region Sorting

        public static Expression<Func<string, string?>> BuildNameToDescriptionMapper(IStringLocalizer settingEnumStringLocalizer)
        {
            var param = Expression.Parameter(typeof(string));
            Expression noTranslationValue = Expression.Constant(null, typeof(string));

            var mapping = EnumMetadata<SettingEnum>.Members.Values
                .Select(metadata =>
                {
                    var description = metadata.Attributes.OfType<DescriptionAttribute>().FirstOrDefault()?.Description;
                    return (metadata.Name, Translation: description != null ? settingEnumStringLocalizer[description] : null);
                })
                .Where(item => item.Translation != null)
                .Aggregate(noTranslationValue, (expression, item) =>
                {
                    // we need to box the translation string to force it to be included as an SQL parameter
                    var translationBoxedAccess = ExpressionHelper.MakeBoxedAccess(item.Translation!.Value);
                    return Expression.Condition(Expression.Equal(param, Expression.Constant(item.Name)), translationBoxedAccess, expression);
                });

            return Expression.Lambda<Func<string, string?>>(mapping, param);
        }

        #endregion

        #region Validation

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
                    case SettingEnum.EnableSwagger:
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

        #endregion
    }
}
