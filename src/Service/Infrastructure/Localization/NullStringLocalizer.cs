using System;
using System.Collections.Generic;
using System.Globalization;
using Karambolo.Common.Localization;
using Microsoft.Extensions.Localization;

namespace WebApp.Service.Infrastructure.Localization
{
    internal sealed class NullStringLocalizer : IStringLocalizer
    {
        public static readonly NullStringLocalizer Instance = new NullStringLocalizer();

        private NullStringLocalizer() { }

        public LocalizedString this[string name] => new LocalizedString(name, name, resourceNotFound: false);

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var plural = (Plural?)Array.Find(arguments, arg => arg is Plural) ?? default;
                var value = plural.Id != null && plural.Count != 1 ? plural.Id : name;
                return new LocalizedString(name, string.Format(value, arguments), resourceNotFound: false);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => throw new NotSupportedException();

        public IStringLocalizer WithCulture(CultureInfo culture) => this;
    }
}
