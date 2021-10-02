using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using WebApp.UI.Models;

namespace WebApp.UI.Areas.Dashboard.Models
{
    public abstract class EditPageDescriptor : PageDescriptor
    {
        public const string EditPopupPartialViewName = "Partials/_EditPopup";

        public EditPageDescriptor()
        {
            GetDefaultTitle = (httpContext, t) => CreatesItem ? t["Create {0}", GetItemDisplayName(httpContext, t)] : t["Edit {0}", GetItemDisplayName(httpContext, t)];
        }

        public abstract bool CreatesItem { get; }
        public virtual Func<HttpContext, IHtmlLocalizer, string> GetItemDisplayName { get; } = (_, t) => t["Item"].Value;
        public override Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetDefaultTitle { get; }
    }

    public abstract class EditPageDescriptor<TItem> : EditPageDescriptor
    {
        public EditPageDescriptor()
        {
            GetItemDisplayName = (httpContext, t) =>
            {
                var modelMetadataProvider = httpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();
                var itemDisplayName = modelMetadataProvider.GetMetadataForType(typeof(TItem))?.DisplayName;
                return itemDisplayName ?? base.GetItemDisplayName(httpContext, t);
            };
        }

        public override Func<HttpContext, IHtmlLocalizer, string> GetItemDisplayName { get; }
    }
}
