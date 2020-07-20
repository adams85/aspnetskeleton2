using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Karambolo.Common.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace WebApp.Service.Infrastructure.Localization
{
    public sealed class CompositeStringLocalizer : IExtendedStringLocalizer
    {
        private readonly IReadOnlyList<IStringLocalizer> _stringLocalizers;
        private readonly CultureInfo? _culture;
        private readonly ILogger _logger;

        private CompositeStringLocalizer(IEnumerable<IStringLocalizer> stringLocalizers, CultureInfo? culture = null, ILogger<CompositeStringLocalizer>? logger = null)
        {
            _stringLocalizers = stringLocalizers.ToArray();
            _culture = culture;
            _logger = logger ?? (ILogger)NullLogger.Instance;
        }

        public CompositeStringLocalizer(IStringLocalizerFactory stringLocalizerFactory, IEnumerable<(string? BaseName, string Location)> baseNameLocationPairs,
            ILogger<CompositeStringLocalizer>? logger = null)
            : this(baseNameLocationPairs.Select(pair => stringLocalizerFactory.Create(pair.BaseName, pair.Location)), null, logger) { }

        public CompositeStringLocalizer(IStringLocalizerFactory stringLocalizerFactory, IEnumerable<Type> types, ILogger<CompositeStringLocalizer>? logger = null)
            : this(types.Select(type => stringLocalizerFactory.Create(type)), null, logger) { }

        private CultureInfo CurrentCulture => _culture ?? CultureInfo.CurrentUICulture;

        public LocalizedString this[string name]
        {
            get
            {
                var resourceNotFound = !TryLocalize(name, out var searchedLocation, out var value);
                if (resourceNotFound)
                {
                    _logger.TranslationNotAvailable(name, CurrentCulture, searchedLocation);
                    NullStringLocalizer.Instance.TryLocalize(name, out var _, out value);
                }
                return new LocalizedString(name, value, resourceNotFound, searchedLocation);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var resourceNotFound = !TryLocalize(name, arguments, out var searchedLocation, out var value);
                if (resourceNotFound)
                {
                    _logger.TranslationNotAvailable(name, CurrentCulture, searchedLocation);
                    NullStringLocalizer.Instance.TryLocalize(name, arguments, out var _, out value);
                }
                return new LocalizedString(name, value, resourceNotFound, searchedLocation);
            }
        }

        private bool TryGetValue<TState>(TState state,
            Func<IStringLocalizer, TState, (string?, string?, bool)> getValue,
            Func<IExtendedStringLocalizer, TState, (string?, string?, bool)> getValueExtended,
            out string? searchedLocation, [MaybeNullWhen(false)] out string value)
        {
            var searchedLocations = new List<string>();

            for (int i = 0, n = _stringLocalizers.Count; i < n; i++)
            {
                var stringLocalizer = _stringLocalizers[i];
                var (searchedLocationLocal, valueLocal, translationFound) =
                    stringLocalizer is IExtendedStringLocalizer extendedStringLocalizer ?
                    getValueExtended(extendedStringLocalizer, state) :
                    getValue(stringLocalizer, state);

                if (searchedLocationLocal != null)
                    searchedLocations.Add(searchedLocationLocal);

                if (translationFound)
                {
                    searchedLocation = GetSearchedLocations(searchedLocations);
                    value = valueLocal!;
                    return true;
                }
            }

            searchedLocation = GetSearchedLocations(searchedLocations);
            value = default;
            return false;

            static string? GetSearchedLocations(List<string> searchedLocations) =>
                searchedLocations.Count > 0 ? string.Join(", ", searchedLocations) : null;
        }

        public string GetTranslation(string name, Plural plural, TextContext context, out string? searchedLocation, out bool resourceNotFound)
        {
            resourceNotFound = !TryGetTranslation(name, plural, context, out searchedLocation, out var value);
            if (resourceNotFound)
            {
                _logger.TranslationNotAvailable(name, CurrentCulture, searchedLocation);
                value = NullStringLocalizer.Instance.GetTranslation(name, plural, context, out var _, out var _);
            }

            return value!;
        }

        public bool TryGetTranslation(string name, Plural plural, TextContext context, out string? searchedLocation, [MaybeNullWhen(false)] out string value) =>
            TryGetValue(
                state: (name, plural, context),
                getValue: (localizer, state) =>
                {
                    var (name, _, _) = state;
                    var localizedString = localizer[name];
                    return (localizedString.SearchedLocation, localizedString.Value, !localizedString.ResourceNotFound);
                },
                getValueExtended: (localizer, state) =>
                {
                    var (name, plural, context) = state;
                    var translationFound = localizer.TryGetTranslation(name, plural, context, out var searchedLocation, out var value);
                    return (searchedLocation, value, translationFound);
                },
                out searchedLocation,
                out value);

        public bool TryLocalize(string name, out string? searchedLocation, [MaybeNullWhen(false)] out string value) =>
            TryGetValue(
                state: name,
                getValue: (localizer, state) =>
                {
                    var localizedString = localizer[state];
                    return (localizedString.SearchedLocation, localizedString.Value, !localizedString.ResourceNotFound);
                },
                getValueExtended: (localizer, state) =>
                {
                    var translationFound = localizer.TryLocalize(state, out var searchedLocation, out var value);
                    return (searchedLocation, value, translationFound);
                },
                out searchedLocation,
                out value);

        public bool TryLocalize(string name, object[] arguments, out string? searchedLocation, [MaybeNullWhen(false)] out string value) =>
            TryGetValue(
                state: (name, arguments),
                getValue: (localizer, state) =>
                {
                    var (name, arguments) = state;
                    var localizedString = localizer[name, arguments];
                    return (localizedString.SearchedLocation, localizedString.Value, !localizedString.ResourceNotFound);
                },
                getValueExtended: (localizer, state) =>
                {
                    var (name, arguments) = state;
                    var translationFound = localizer.TryLocalize(name, arguments, out var searchedLocation, out var value);
                    return (searchedLocation, value, translationFound);
                },
                out searchedLocation,
                out value);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            _stringLocalizers.SelectMany(localizer => localizer.GetAllStrings());

        [Obsolete("This method is obsolete. Use `CurrentCulture` and `CurrentUICulture` instead.")]
        public IStringLocalizer WithCulture(CultureInfo culture) =>
            new CompositeStringLocalizer(_stringLocalizers.Select(localizer => localizer.WithCulture(culture)), _culture, _logger as ILogger<CompositeStringLocalizer>);
    }
}
