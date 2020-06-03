using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.Common.Settings;
using WebApp.Service.Contract.Settings;

namespace WebApp.Service.Settings
{
    internal class UpdateSettingCommandHandler : CommandHandler<UpdateSettingCommand>
    {
        public override async Task HandleAsync(UpdateSettingCommand command, CommandContext context, CancellationToken cancellationToken)
        {
            var setting = await context.DbContext.Settings
                .FirstOrDefaultAsync(setting => setting.Name == command.Name, cancellationToken).ConfigureAwait(false);

            RequireExisting(setting, c => c.Name);

            var value = command.Value;

            if (Enum.TryParse<SettingEnum>(command.Name, ignoreCase: false, out var settingEnum))
                switch (settingEnum)
                {
                    case SettingEnum.MaxPageSize:
                    {
                        var minValue = int.TryParse(setting.MinValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue) ? intValue : (int?)null;
                        var maxValue = int.TryParse(setting.MaxValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out intValue) ? intValue : (int?)null;

                        RequireValid(int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out intValue) &&
                            (minValue == null || minValue.Value <= intValue) &&
                            (maxValue == null || intValue <= maxValue.Value), c => c.Value);

                        break;
                    }
                }

            setting.Value = value;

            await context.DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
