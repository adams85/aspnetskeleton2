using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using WebApp.Common.Settings;
using WebApp.DataAccess.Entities;
using WebApp.Service.Helpers;

namespace WebApp.Service.Infrastructure.Database
{
    public partial class DbInitializer
    {
        public async Task SeedSettingsAsync(CancellationToken cancellationToken)
        {
            var settings = await _context.Settings.ToDictionarySafeAsync(entity => entity.Name, AsExistingEntity, _caseSensitiveComparer, cancellationToken).ConfigureAwait(false);

            foreach (var settingName in Enum.GetNames(typeof(SettingEnum)))
            {
                var field = typeof(SettingEnum)
                    .GetField(settingName, BindingFlags.Public | BindingFlags.Static);

                var descriptionAttribute = field.GetAttributes<DescriptionAttribute>().FirstOrDefault();
                var defaultValueAttribute = field.GetAttributes<DefaultValueAttribute>().FirstOrDefault();
                var rangeAttribute = field.GetAttributes<RangeAttribute>().FirstOrDefault();

                AddOrUpdateSetting(settings,
                    settingName,
                    defaultValue: Convert.ToString(defaultValueAttribute?.Value, CultureInfo.InvariantCulture),
                    minValue: Convert.ToString(rangeAttribute?.Minimum, CultureInfo.InvariantCulture),
                    maxValue: Convert.ToString(rangeAttribute?.Maximum, CultureInfo.InvariantCulture));
            }

            _context.Settings.AddRange(GetEntitesToAdd(settings.Values));
            _context.Settings.RemoveRange(GetEntitesToRemove(settings.Values));
        }

        private static void AddOrUpdateSetting(Dictionary<string, EntityInfo<Setting>> settings, string name, string? defaultValue, string? minValue, string? maxValue)
        {
            if (!settings.TryGetValue(name, out var setting))
                settings.Add(name, setting = AsNewEntity(new Setting
                {
                    Name = name,
                    Value = defaultValue,
                }));
            else
                setting.State = EntityState.Seen;

            setting.Entity.DefaultValue = defaultValue;
            setting.Entity.MinValue = minValue;
            setting.Entity.MaxValue = maxValue;
        }
    }
}
