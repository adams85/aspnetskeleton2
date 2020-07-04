using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Localization;
using WebApp.Common.Infrastructure.Localization;

namespace WebApp.Api.Infrastructure.Localization
{
    public sealed class StringLocalizerAdapter : IStringLocalizer, ITextLocalizer
    {
        private readonly IStringLocalizer _stringLocalizer;

        public StringLocalizerAdapter(IStringLocalizer stringLocalizer)
        {
            _stringLocalizer = stringLocalizer;
        }

        public LocalizedString this[string name] => _stringLocalizer[name];
        string ITextLocalizer.this[string hint] => _stringLocalizer[hint].Value;

        public LocalizedString this[string name, params object[] arguments] => _stringLocalizer[name, arguments];
        string ITextLocalizer.this[string hint, params object[] args] => _stringLocalizer[hint, args].Value;

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => _stringLocalizer.GetAllStrings(includeParentCultures);

#pragma warning disable CS0618 // Type or member is obsolete
        public IStringLocalizer WithCulture(CultureInfo culture) => new StringLocalizerAdapter(_stringLocalizer.WithCulture(culture));
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
