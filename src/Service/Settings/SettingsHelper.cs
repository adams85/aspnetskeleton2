using System;
using System.Linq;
using System.Linq.Expressions;
using WebApp.DataAccess.Entities;
using WebApp.Service.Contract.Settings;

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
    }
}
