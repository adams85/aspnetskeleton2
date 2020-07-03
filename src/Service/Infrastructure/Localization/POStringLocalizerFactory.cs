using System;
using Microsoft.Extensions.Localization;
using WebApp.Service.Translations;

namespace WebApp.Service.Infrastructure.Localization
{
    internal sealed class POStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ITranslationsProvider _translationsProvider;

        public POStringLocalizerFactory(ITranslationsProvider translationsProvider)
        {
            _translationsProvider = translationsProvider ?? throw new ArgumentNullException(nameof(translationsProvider));
        }

        public IStringLocalizer Create(Type resourceSource) => Create(null, resourceSource.Assembly.GetName().Name);

        public IStringLocalizer Create(string? baseName, string location) => new POStringLocalizer(_translationsProvider, location);
    }
}
