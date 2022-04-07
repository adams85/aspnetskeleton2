using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Translations;

internal interface ITranslationsSource
{
    Task<TranslationsChangedEvent[]> GetLatestVersionAsync(CancellationToken cancellationToken);
    void Invalidate(string? location, string? culture);
}
