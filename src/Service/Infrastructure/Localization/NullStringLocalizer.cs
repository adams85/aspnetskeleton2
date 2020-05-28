using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Localization;

namespace WebApp.Service.Infrastructure.Localization
{
    public sealed class NullStringLocalizer : IStringLocalizer
    {
        public static readonly NullStringLocalizer Instance = new NullStringLocalizer();

        private NullStringLocalizer() { }

        public LocalizedString this[string name] => new LocalizedString(name, name);

        public LocalizedString this[string name, params object[] arguments] => new LocalizedString(name, string.Format(name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => throw new NotSupportedException();

        public IStringLocalizer WithCulture(CultureInfo culture) => throw new NotSupportedException();
    }
}
