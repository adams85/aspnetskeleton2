using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Karambolo.Common.Localization;
using Karambolo.PO;
using Microsoft.Extensions.Localization;
using WebApp.Service.Translations;

namespace WebApp.Service.Infrastructure.Localization
{
    internal sealed class POStringLocalizer : IStringLocalizer
    {
        private readonly ITranslationsProvider _translationsProvider;
        private readonly string _location;
        private readonly CultureInfo? _culture;

        public POStringLocalizer(ITranslationsProvider translationsProvider, string location, CultureInfo? culture = null)
        {
            _translationsProvider = translationsProvider;
            _location = location;
            _culture = culture;
        }

        private bool TryGetTranslation(string id, Plural plural, TextContext context, [MaybeNullWhen(false)] out string translation)
        {
            var catalogs = _translationsProvider.GetCatalogs();

            var catalog = GetCatalog(catalogs, _location, _culture ?? CultureInfo.CurrentUICulture);
            if (catalog != null)
            {
                var key = new POKey(id, plural.Id, context.Id);
                translation = plural.Id == null ? catalog.GetTranslation(key) : catalog.GetTranslation(key, plural.Count);
                if (translation != null)
                    return true;
            }

            translation = default;
            return false;

            static POCatalog? GetCatalog(IReadOnlyDictionary<(string, string), POCatalog> catalogs, string location, CultureInfo culture)
            {
                for (; ; )
                {
                    if (catalogs.TryGetValue((location, culture.Name), out var catalog))
                        return catalog;

                    var parentCulture = culture.Parent;
                    if (culture == parentCulture)
                        return null;

                    culture = parentCulture;
                }
            }
        }

        public LocalizedString this[string name]
        {
            get
            {
                var translationFound = TryGetTranslation(name, default, default, out var value);
                // TODO: log when not found
                if (!translationFound)
                    value = name;

                return new LocalizedString(name, value, !translationFound, _location);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var plural = (Plural?)Array.Find(arguments, arg => arg is Plural) ?? default;
                var context = arguments.Length > 0 ? (arguments[^1] as TextContext? ?? default) : default;

                var translationFound = TryGetTranslation(name, plural, context, out var value);
                // TODO: log when not found
                if (!translationFound)
                    value = plural.Id != null && plural.Count != 1 ? plural.Id : name;

                return new LocalizedString(name, string.Format(value, arguments), !translationFound, _location);
            }
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
