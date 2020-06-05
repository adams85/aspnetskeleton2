using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using WebApp.Common.Settings;

namespace WebApp.Service.Settings
{
    public static class SettingsProviderExtensions
    {
        public const int FallbackMaxPageSize = 200;

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

        public static int GetMaxPageSize(this ISettingsProvider provider) =>
            provider.Get<int?>(SettingEnum.MaxPageSize) ?? FallbackMaxPageSize;
    }
}
