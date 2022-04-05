using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.Common.Settings;
using WebApp.Core.Helpers;
using WebApp.DataAccess;
using WebApp.DataAccess.Entities;
using WebApp.Service.Helpers;

namespace WebApp.Service.Infrastructure.Database
{
    public partial class DbInitializer
    {
        public async Task SeedSettingsAsync(WritableDataContext dbContext, CancellationToken cancellationToken)
        {
            var dbProperties = dbContext.GetDbProperties();
            var settings = await dbContext.Settings.AsAsyncEnumerable().ToDictionarySafeAsync(entity => entity.Name, AsExistingEntity, dbProperties.CaseSensitiveComparer, cancellationToken).ConfigureAwait(false);

            foreach (var (settingName, enumMetadata) in EnumMetadata<SettingEnum>.Members)
            {
                var field = typeof(SettingEnum)
                    .GetField(settingName, BindingFlags.Public | BindingFlags.Static);

                var descriptionAttribute = enumMetadata.Attributes.OfType<DescriptionAttribute>().FirstOrDefault();
                var defaultValueAttribute = enumMetadata.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault();
                var rangeAttribute = enumMetadata.Attributes.OfType<RangeAttribute>().FirstOrDefault();

                AddOrUpdateSetting(settings,
                    settingName,
                    defaultValue: Convert.ToString(defaultValueAttribute?.Value, CultureInfo.InvariantCulture),
                    minValue: Convert.ToString(rangeAttribute?.Minimum, CultureInfo.InvariantCulture),
                    maxValue: Convert.ToString(rangeAttribute?.Maximum, CultureInfo.InvariantCulture));
            }

            dbContext.Settings.AddRange(GetEntitesToAdd(settings.Values));
            dbContext.Settings.RemoveRange(GetEntitesToRemove(settings.Values));
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
