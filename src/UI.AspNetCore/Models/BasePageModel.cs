using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace WebApp.UI.Models
{
    public abstract class BasePageModel : PageModel, IPageDescriptorProvider
    {
        PageDescriptor IPageDescriptorProvider.PageDescriptor => GetPageDescriptor();

        protected abstract PageDescriptor GetPageDescriptor();

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

    [StaticPageDescriptorProvider(nameof(PageDescriptor))]
    public abstract class BasePageModel<TPageDescriptor> : BasePageModel
        where TPageDescriptor : PageDescriptor, new()
    {
        public static readonly TPageDescriptor PageDescriptor = new TPageDescriptor();

        protected sealed override PageDescriptor GetPageDescriptor() => PageDescriptor;
    }
}
