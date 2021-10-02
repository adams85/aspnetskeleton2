using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.UI.Models;

namespace WebApp.UI.Pages
{
    public class IndexModel : BasePageModel<IndexModel.PageDescriptorClass>
    {
        public IActionResult OnGet()
        {
            return RedirectToPage(Areas.Dashboard.Pages.IndexModel.PageDescriptor.PageName, new { area = Areas.Dashboard.Pages.IndexModel.PageDescriptor.AreaName });
        }

        public sealed class PageDescriptorClass : PageDescriptor
        {
            public override string PageName => "/Index";
            public override Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetDefaultTitle => throw new NotSupportedException();
        }
    }
}
