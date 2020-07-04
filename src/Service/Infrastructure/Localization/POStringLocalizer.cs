using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Karambolo.Common.Localization;
using Karambolo.PO;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WebApp.Service.Translations;

namespace WebApp.Service.Infrastructure.Localization
{
    internal sealed class POStringLocalizer : IExtendedStringLocalizer
    {
        private readonly ITranslationsProvider _translationsProvider;
        private readonly string _location;
        private readonly CultureInfo? _culture;
        private readonly ILogger _logger;

        public POStringLocalizer(ITranslationsProvider translationsProvider, string location, CultureInfo? culture = null, ILogger<POStringLocalizer>? logger = null)
        {
            _translationsProvider = translationsProvider;
            _location = location;
            _culture = culture;
            _logger = logger ?? (ILogger)NullLogger.Instance;
        }

        public LocalizedString this[string name]
        {
            get
            {
                var translationFound = TryLocalize(name, out var searchedLocation, out var value);
                if (!translationFound)
                {
                    _logger.TranslationNotFound(name, searchedLocation);
                    NullStringLocalizer.Instance.TryLocalize(name, out var _, out value);
                }
                return new LocalizedString(name, value, resourceNotFound: !translationFound, searchedLocation);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var translationFound = TryLocalize(name, arguments, out var searchedLocation, out var value);
                if (!translationFound)
                {
                    _logger.TranslationNotFound(name, searchedLocation);
                    NullStringLocalizer.Instance.TryLocalize(name, arguments, out var _, out value);
                }
                return new LocalizedString(name, value, resourceNotFound: !translationFound, searchedLocation);
            }
        }

        private POCatalog? GetCatalog()
        {
            var catalogs = _translationsProvider.GetCatalogs();
            var culture = _culture ?? CultureInfo.CurrentUICulture;
            for (; ; )
            {
                if (catalogs.TryGetValue((_location, culture.Name), out var catalog))
                    return catalog;

                var parentCulture = culture.Parent;
                if (culture == parentCulture)
                    return null;

                culture = parentCulture;
            }
        }

        private bool TryGetTranslation(string id, Plural plural, TextContext context, [MaybeNullWhen(false)] out string value)
        {
            var catalog = GetCatalog();
            if (catalog != null)
            {
                var key = new POKey(id, plural.Id, context.Id);
                value = plural.Id == null ? catalog.GetTranslation(key) : catalog.GetTranslation(key, plural.Count);
                if (value != null)
                    return true;
            }

            value = default;
            return false;
        }

        public bool TryLocalize(string name, out string? searchedLocation, [MaybeNullWhen(false)] out string value)
        {
            searchedLocation = _location;
            return TryGetTranslation(name, default, default, out value);
        }

        public bool TryLocalize(string name, object[] arguments, out string? searchedLocation, [MaybeNullWhen(false)] out string value)
        {
            searchedLocation = _location;

            var (plural, context) = LocalizationHelper.GetSpecialArgs(arguments);
            if (!TryGetTranslation(name, plural, context, out value))
                return false;

            value = string.Format(value, arguments);
            return true;
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var catalogs = _translationsProvider.GetCatalogs();
            var culture = _culture ?? CultureInfo.CurrentUICulture;
            do
            {
                if (catalogs.TryGetValue((_location, culture.Name), out var catalog))
                    foreach (var entry in catalog)
                        if (entry.Count > 0)
                            yield return new LocalizedString(entry.Key.Id, entry[0], resourceNotFound: false, _location);

                var parentCulture = culture.Parent;
                if (culture == parentCulture)
                    break;

                culture = parentCulture;
            }
            while (includeParentCultures);
        }

        public IStringLocalizer WithCulture(CultureInfo? culture) => new POStringLocalizer(_translationsProvider, _location, culture);
    }
}
