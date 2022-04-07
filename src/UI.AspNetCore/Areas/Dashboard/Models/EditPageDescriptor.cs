using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using WebApp.UI.Models;

namespace WebApp.UI.Areas.Dashboard.Models;

public abstract class EditPageDescriptor : PageDescriptor
{
    public const string EditPopupPartialViewName = "Partials/_EditPopup";

    public abstract bool CreatesItem { get; }

    public virtual string GetItemDisplayName(HttpContext httpContext, IHtmlLocalizer t) =>
        t["Item"].Value;

    public override LocalizedHtmlString GetDefaultTitle(HttpContext httpContext, IHtmlLocalizer t) =>
        CreatesItem ? t["Create {0}", GetItemDisplayName(httpContext, t)] : t["Edit {0}", GetItemDisplayName(httpContext, t)];
}

public abstract class EditPageDescriptor<TItem> : EditPageDescriptor
{
    public override string GetItemDisplayName(HttpContext httpContext, IHtmlLocalizer t)
    {
        var modelMetadataProvider = httpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();
        var itemDisplayName = modelMetadataProvider.GetMetadataForType(typeof(TItem))?.DisplayName;
        return itemDisplayName ?? base.GetItemDisplayName(httpContext, t);
    }
}
