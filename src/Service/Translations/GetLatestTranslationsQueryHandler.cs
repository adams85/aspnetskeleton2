using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Translations;

internal sealed class GetLatestTranslationsQueryHandler : QueryHandler<GetLatestTranslationsQuery, TranslationsChangedEvent[]?>
{
    private readonly ITranslationsSource _translationsSource;

    public GetLatestTranslationsQueryHandler(ITranslationsSource translationsSource)
    {
        _translationsSource = translationsSource ?? throw new ArgumentNullException(nameof(translationsSource));
    }

    public override async Task<TranslationsChangedEvent[]?> HandleAsync(GetLatestTranslationsQuery query, QueryContext context, CancellationToken cancellationToken)
    {
        return await _translationsSource.GetLatestVersionAsync(cancellationToken).ConfigureAwait(false);
    }
}
