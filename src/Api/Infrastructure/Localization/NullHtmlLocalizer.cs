using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.Service.Infrastructure.Localization;

#if SERVICE_HOST
namespace WebApp.Service.Host.Infrastructure.Localization
#else
namespace WebApp.Api.Infrastructure.Localization
#endif
{
    public sealed class NullHtmlLocalizer : HtmlLocalizer
    {
        public static readonly NullHtmlLocalizer Instance = new NullHtmlLocalizer();

        private NullHtmlLocalizer() : base(NullStringLocalizer.Instance) { }

        public override LocalizedHtmlString this[string name]
        {
            get
            {
                var translation = NullStringLocalizer.Instance.GetTranslation(name, default, default, out var _, out var resourceNotFound);
                return new LocalizedHtmlString(name, translation, resourceNotFound);
            }
        }

        public override LocalizedHtmlString this[string name, params object[] arguments]
        {
            get
            {
                var (plural, context) = LocalizationHelper.GetSpecialArgs(arguments);
                var translation = NullStringLocalizer.Instance.GetTranslation(name, plural, context, out var _, out var resourceNotFound);
                return new LocalizedHtmlString(name, translation, resourceNotFound, arguments);
            }
        }
    }
}
