using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace WebApp.Service.Infrastructure.Localization
{
    public sealed class CompositeStringLocalizer : IExtendedStringLocalizer
    {
        private readonly IReadOnlyList<IStringLocalizer> _stringLocalizers;
        private readonly ILogger _logger;

        public CompositeStringLocalizer(IEnumerable<IStringLocalizer> stringLocalizers, ILogger<CompositeStringLocalizer>? logger = null)
        {
            _stringLocalizers = stringLocalizers.ToArray();
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
                    NullStringLocalizer.Instance.TryLocalize(name, out searchedLocation, out value);
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
                    NullStringLocalizer.Instance.TryLocalize(name, arguments, out searchedLocation, out value);
                }
                return new LocalizedString(name, value, resourceNotFound: !translationFound, searchedLocation);
            }
        }

        private bool TryLocalize(string name, object[] arguments,
            Func<IStringLocalizer, string, object[], LocalizedString> getTranslation,
            Func<IExtendedStringLocalizer, string, object[], (string?, string, bool)> tryLocalize,
            out string? searchedLocation, [MaybeNullWhen(false)] out string value)
        {
            value = default!;

            var success = false;
            var searchedLocations = new List<string>();

            for (int i = 0, n = _stringLocalizers.Count; i < n; i++)
            {
                var stringLocalizer = _stringLocalizers[i];
                if (stringLocalizer is IExtendedStringLocalizer extendedStringLocalizer)
                {
                    var (searchedLocationLocal, valueLocal, translationFound) = tryLocalize(extendedStringLocalizer, name, arguments);

                    if (searchedLocationLocal != null)
                        searchedLocations.Add(searchedLocationLocal);

                    if (translationFound)
                    {
                        value = valueLocal;
                        success = true;
                        break;
                    }
                }
                else
                {
                    var localizedString = getTranslation(stringLocalizer, name, arguments);

                    if (localizedString.SearchedLocation != null)
                        searchedLocations.Add(localizedString.SearchedLocation);

                    if (!localizedString.ResourceNotFound)
                    {
                        value = localizedString.Value;
                        success = true;
                        break;
                    }
                }
            }

            searchedLocation = string.Join(", ", searchedLocations);
            return success;
        }

        public bool TryLocalize(string name, out string? searchedLocation, [MaybeNullWhen(false)] out string value) =>
            TryLocalize(name, default!,
                (localizer, name, _) => localizer[name],
                (localizer, name, _) =>
                {
                    var translationFound = localizer.TryLocalize(name, out var searchedLocation, out var value);
                    return (searchedLocation, value!, translationFound);
                },
                out searchedLocation, out value);

        public bool TryLocalize(string name, object[] arguments, out string? searchedLocation, [MaybeNullWhen(false)] out string value) =>
            TryLocalize(name, arguments,
                (localizer, name, args) => localizer[name, args],
                (localizer, name, args) =>
                {
                    var translationFound = localizer.TryLocalize(name, args, out var searchedLocation, out var value);
                    return (searchedLocation, value!, translationFound);
                },
                out searchedLocation, out value);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            _stringLocalizers.SelectMany(localizer => localizer.GetAllStrings());

        public IStringLocalizer WithCulture(CultureInfo culture) =>
#pragma warning disable CS0618 // Type or member is obsolete
            new CompositeStringLocalizer(_stringLocalizers.Select(localizer => localizer.WithCulture(culture)));
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
