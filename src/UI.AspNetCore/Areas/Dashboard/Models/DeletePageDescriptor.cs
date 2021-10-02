using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using WebApp.UI.Models;

namespace WebApp.UI.Areas.Dashboard.Models
{
    public abstract class DeletePageDescriptor : PageDescriptor
    {
        public const string DeletePopupPartialViewName = "Partials/_DeletePopup";
        public const string DeleteConfirmationPartialViewName = "Partials/_DeleteConfirmation";

        public DeletePageDescriptor()
        {
            GetDefaultTitle = (httpContext, t) => t["Delete {0}", GetItemDisplayName(httpContext, t)];
        }

        public virtual Func<HttpContext, IHtmlLocalizer, string> GetItemDisplayName { get; } = (_, t) => t["Item"].Value;
        public override Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetDefaultTitle { get; }
    }

    public abstract class DeletePageDescriptor<TItem> : DeletePageDescriptor
    {
        public DeletePageDescriptor()
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
