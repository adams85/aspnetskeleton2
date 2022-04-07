using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.UI.Models;

namespace WebApp.UI.Areas.Dashboard.Models.Layout
{
    public class DashboardSidebarModel
    {
        public const string SidebarStateCookieName = "App.Dashboard.SidebarState";

        public static (bool IsVisible, bool IsMinimized) GetSidebarState(HttpContext httpContext) =>
            int.TryParse(httpContext.Request.Cookies[SidebarStateCookieName], NumberStyles.Integer, CultureInfo.InvariantCulture, out var state) ?
            ((state & 0x1) != 0, (state & 0x2) != 0) :
            (true, false);

        public List<NavigationGroup>? Groups { get; init; }

        public class NavigationGroup
        {
            public Func<HttpContext, Task<bool>>? IsVisibleAsync { get; init; }
            public Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString>? GetTitle { get; init; }

            private List<NavigationItemBase>? _items;
            public List<NavigationItemBase> Items
            {
                get => _items ??= new List<NavigationItemBase>();
                init => _items = value;
            }
        }

        public abstract class NavigationItemBase
        {
            public Func<HttpContext, Task<bool>>? IsVisibleAsync { get; init; }
            public Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetTitle { get; init; } = null!;
            public string? IconCssClass { get; init; }
        }

        public class NavigationItem : NavigationItemBase
        {
            public NavigationItem() { }

            public NavigationItem(PageDescriptor page)
            {
                IsVisibleAsync = page.IsAccessAllowedAsync;
                GetTitle = page.GetDefaultTitle;
                GetUrl = urlHelper => urlHelper.Page(page.PageName, new { area = page.AreaName });
                IsActive = (_, currentPage) => currentPage == page;
            }

            public Func<IUrlHelper, string?> GetUrl { get; init; } = null!;
            public Func<HttpContext, PageDescriptor?, bool> IsActive { get; init; } = null!;
        }

        public class NavigationDropDownItem : NavigationItemBase
        {
            private List<NavigationItemBase>? _items;
            public List<NavigationItemBase> Items
            {
                get => _items ??= new List<NavigationItemBase>();
                init => _items = value;
            }

            public bool IsShown(HttpContext context, PageDescriptor? currentPage)
            {
                return Items.Any(itemBase =>
                    itemBase is NavigationItem item && item.IsActive(context, currentPage) ||
                    itemBase is NavigationDropDownItem dropDownItem && dropDownItem.IsShown(context, currentPage));
            }
        }
    }
}
