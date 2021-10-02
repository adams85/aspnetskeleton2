using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.UI.Models;

namespace WebApp.UI.Pages.Account
{
    public class AccessDeniedModel : CardPageModel<AccessDeniedModel.PageDescriptorClass>
    {
        public void OnGet() { }

        public sealed class PageDescriptorClass : PageDescriptor
        {
            public override string PageName => "/Account/AccessDenied";
            public override Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetDefaultTitle { get; } = (_, t) => t["Access Denied"];
        }
    }
}
