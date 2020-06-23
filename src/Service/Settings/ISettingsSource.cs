using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Settings
{
    internal interface ISettingsSource
    {
        Task<SettingsChangedEvent> GetLatestVersionAsync(CancellationToken cancellationToken);
        void Invalidate();
    }
}
