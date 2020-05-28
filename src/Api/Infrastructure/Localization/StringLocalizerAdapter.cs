using Microsoft.Extensions.Localization;
using WebApp.Common.Infrastructure.Localization;

namespace WebApp.Api.Infrastructure.Localization
{
    public sealed class StringLocalizerAdapter : ITextLocalizer
    {
        public StringLocalizerAdapter(IStringLocalizer stringLocalizer)
        {
            StringLocalizer = stringLocalizer;
        }

        public IStringLocalizer StringLocalizer { get; }

        public string this[string hint] => StringLocalizer[hint].Value;

        public string this[string hint, params object[] args] => StringLocalizer[hint, args].Value;
    }
}
