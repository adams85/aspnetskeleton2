using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using WebApp.UI.Models;

namespace WebApp.UI.Areas.Dashboard.Models;

public abstract class DeletePageDescriptor : PageDescriptor
{
    public const string DeletePopupPartialViewName = "Partials/_DeletePopup";
    public const string DeleteConfirmationPartialViewName = "Partials/_DeleteConfirmation";

    public virtual string GetItemDisplayName(HttpContext httpContext, IHtmlLocalizer t) =>
        t["Item"].Value;

    public override LocalizedHtmlString GetDefaultTitle(HttpContext httpContext, IHtmlLocalizer t) =>
        t["Delete {0}", GetItemDisplayName(httpContext, t)];
}

public abstract class DeletePageDescriptor<TItem> : DeletePageDescriptor
{
    public override string GetItemDisplayName(HttpContext httpContext, IHtmlLocalizer t)
    {
        var modelMetadataProvider = httpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();
        var itemDisplayName = modelMetadataProvider.GetMetadataForType(typeof(TItem))?.DisplayName;
        return itemDisplayName ?? base.GetItemDisplayName(httpContext, t);
    }
}
