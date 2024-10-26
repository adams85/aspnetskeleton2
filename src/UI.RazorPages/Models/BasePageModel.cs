using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace WebApp.UI.Models;

public abstract class BasePageModel : PageModel
{
    public virtual NoContentResult NoContent()
    {
        return new NoContentResult();
    }

    public virtual PartialViewResult PartialWithCurrentViewData(string viewName, object model)
    {
        var viewData = new ViewDataDictionary<object>(ViewData, model);

        return new PartialViewResult
        {
            ViewName = viewName,
            ViewData = viewData
        };
    }
}

public abstract class BasePageModel<TPageDescriptor> : BasePageModel, IPageDescriptorProvider
    where TPageDescriptor : PageDescriptor, new()
{
    public static TPageDescriptor PageDescriptor { get; } = new TPageDescriptor();

    static PageDescriptor IPageDescriptorProvider.PageDescriptorStatic => PageDescriptor;

    PageDescriptor IPageDescriptorProvider.PageDescriptor => PageDescriptor;
}
