using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.Helpers;

namespace WebApp.Service.Settings;

internal sealed class UpdateSettingCommandHandler : CommandHandler<UpdateSettingCommand>
{
    private readonly ISettingsSource _settingsSource;

    public UpdateSettingCommandHandler(ISettingsSource settingsSource)
    {
        _settingsSource = settingsSource ?? throw new ArgumentNullException(nameof(settingsSource));
    }

    public override async Task HandleAsync(UpdateSettingCommand command, CommandContext context, CancellationToken cancellationToken)
    {
        await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
        {
            var setting = await dbContext.Settings
                .FirstOrDefaultAsync(setting => setting.Name == command.Name, cancellationToken).ConfigureAwait(false);

            RequireExisting(setting, c => c.Name);

            var value = command.Value;

            RequireValid(setting.Validate(ref value), c => c.Value);

            setting.Value = value;

            var changeCount = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            if (changeCount > 0)
                _settingsSource.Invalidate();
        }
    }
}
