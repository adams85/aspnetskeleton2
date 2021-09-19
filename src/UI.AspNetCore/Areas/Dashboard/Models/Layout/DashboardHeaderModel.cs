using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.UI.Infrastructure.Navigation;

namespace WebApp.UI.Areas.Dashboard.Models.Layout
{
    public class DashboardHeaderModel
    {
        public List<DropDownMenuItemBase>? UserMenu { get; set; }

        public abstract class DropDownMenuItemBase
        {
            public Func<HttpContext, Task<bool>>? IsVisibleAsync { get; set; }
        }

        public class DropDownMenuItem : DropDownMenuItemBase
        {
            public DropDownMenuItem() { }

            public DropDownMenuItem(PageInfo page)
            {
                IsVisibleAsync = page.IsAccessAllowedAsync;
                GetTitle = page.GetDefaultTitle;
                GetUrl = urlHelper => urlHelper.RouteUrl(page.RouteName);
            }

            public Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetTitle { get; set; } = null!;
            public Func<IUrlHelper, string> GetUrl { get; set; } = null!;
            public string? IconClassName { get; set; }
        }

        public class DropDownMenuHeader : DropDownMenuItemBase
        {
            public Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetTitle { get; set; } = null!;
        }

        public class DropDownMenuDivider : DropDownMenuItemBase { }
    }
}
