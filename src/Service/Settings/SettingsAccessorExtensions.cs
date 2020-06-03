using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using WebApp.Common.Settings;

namespace WebApp.Service.Settings
{
    public static class SettingsAccessorExtensions
    {
        public const int FallbackMaxPageSize = 200;

        [return: MaybeNull]
        public static T Get<T>(this ISettingsAccessor accessor, SettingEnum setting)
        {
            Type? type = typeof(T);

            if (type.IsValueType)
            {
                type = Nullable.GetUnderlyingType(typeof(T));
                if (type == null)
                    throw new ArgumentException("Type parameter must be a type accepting null values.", nameof(T));
            }

            var value = accessor[setting.ToString()];
            return value != null ? (T)Convert.ChangeType(value, type, CultureInfo.InvariantCulture) : default;
        }

        public static int GetMaxPageSize(this ISettingsAccessor accessor) =>
            accessor.Get<int?>(SettingEnum.MaxPageSize) ?? FallbackMaxPageSize;
    }
}
