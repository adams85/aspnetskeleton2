using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Settings;

internal sealed class GetLatestSettingsQueryHandler : QueryHandler<GetLatestSettingsQuery, SettingsChangedEvent>
{
    private readonly ISettingsSource _settingsSource;

    public GetLatestSettingsQueryHandler(ISettingsSource settingsSource)
    {
        _settingsSource = settingsSource ?? throw new ArgumentNullException(nameof(settingsSource));
    }

    public override Task<SettingsChangedEvent> HandleAsync(GetLatestSettingsQuery query, QueryContext context, CancellationToken cancellationToken)
    {
        return _settingsSource.GetLatestVersionAsync(cancellationToken);
    }
}
