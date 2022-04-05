using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Localization;
using WebApp.Common.Infrastructure.Localization;

namespace WebApp.Api.Infrastructure.Localization
{
    public sealed class TextLocalizerAdapter : IStringLocalizer, ITextLocalizer
    {
        private readonly IStringLocalizer _stringLocalizer;

        public TextLocalizerAdapter(IStringLocalizer stringLocalizer)
        {
            _stringLocalizer = stringLocalizer;
        }

        public LocalizedString this[string name] => _stringLocalizer[name];
        string ITextLocalizer.this[string hint] => _stringLocalizer[hint].Value;

        public LocalizedString this[string name, params object[] arguments] => _stringLocalizer[name, arguments];
        string ITextLocalizer.this[string hint, params object[] args] => _stringLocalizer[hint, args].Value;

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => _stringLocalizer.GetAllStrings(includeParentCultures);
    }
}
