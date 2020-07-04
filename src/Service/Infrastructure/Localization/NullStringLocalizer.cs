using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Karambolo.Common.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace WebApp.Service.Infrastructure.Localization
{
    internal sealed class NullStringLocalizer : IExtendedStringLocalizer
    {
        public static readonly NullStringLocalizer Instance = new NullStringLocalizer();

        private NullStringLocalizer() { }

        public LocalizedString this[string name]
        {
            get
            {
                var translationFound = TryLocalize(name, out var searchedLocation, out var value);
                return new LocalizedString(name, value, resourceNotFound: !translationFound, searchedLocation);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var translationFound = TryLocalize(name, arguments, out var searchedLocation, out var value);
                return new LocalizedString(name, value, resourceNotFound: !translationFound, searchedLocation);
            }
        }

        public bool TryLocalize(string name, out string? searchedLocation, [MaybeNullWhen(false)] out string value)
        {
            searchedLocation = null;
            value = name;
            return true;
        }

        public bool TryLocalize(string name, object[] arguments, out string? searchedLocation, [MaybeNullWhen(false)] out string value)
        {
            searchedLocation = null;
            var (plural, _) = LocalizationHelper.GetSpecialArgs(arguments);
            value = string.Format(plural.Id == null || plural.Count == 1 ? name : plural.Id, arguments);
            return true;
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => throw new NotSupportedException();

        public IStringLocalizer WithCulture(CultureInfo culture) => this;
    }
}
