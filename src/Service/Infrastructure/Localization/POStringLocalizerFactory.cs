using System;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WebApp.Service.Translations;

namespace WebApp.Service.Infrastructure.Localization
{
    public sealed class POStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ITranslationsProvider _translationsProvider;
        private readonly ILoggerFactory _loggerFactory;

        public POStringLocalizerFactory(ITranslationsProvider translationsProvider, ILoggerFactory? loggerFactory)
        {
            _translationsProvider = translationsProvider ?? throw new ArgumentNullException(nameof(translationsProvider));
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        }

        public IStringLocalizer Create(Type resourceSource) => Create(null, resourceSource.Assembly.GetName().Name);

        public IStringLocalizer Create(string? baseName, string location) => new POStringLocalizer(_translationsProvider, location, logger: _loggerFactory.CreateLogger<POStringLocalizer>());
    }
}
