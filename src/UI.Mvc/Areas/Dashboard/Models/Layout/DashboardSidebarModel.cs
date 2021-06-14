using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.UI.Infrastructure.Navigation;

namespace WebApp.UI.Areas.Dashboard.Models.Layout
{
    public class DashboardSidebarModel
    {
        public static readonly string SidebarStateCookieName = "App.Dashboard.SidebarState";

        public static (bool IsVisible, bool IsMinimized) GetSidebarState(HttpContext httpContext) =>
            int.TryParse(httpContext.Request.Cookies[SidebarStateCookieName], out var state) ?
            ((state & 0x1) != 0, (state & 0x2) != 0) :
            (true, false);

        public List<NavigationGroup>? Groups { get; set; }

        public class NavigationGroup
        {
            public Func<HttpContext, Task<bool>>? IsVisibleAsync { get; set; }
            public Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString>? GetTitle { get; set; }

            private List<NavigationItemBase>? _items;
            public List<NavigationItemBase> Items
            {
                get => _items ??= new List<NavigationItemBase>();
                set => _items = value;
            }
        }

        public abstract class NavigationItemBase
        {
            public Func<HttpContext, Task<bool>>? IsVisibleAsync { get; set; }
            public Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetTitle { get; set; } = null!;
            public string? IconClassName { get; set; }
        }

        public class NavigationItem : NavigationItemBase
        {
            public NavigationItem() { }

            public NavigationItem(PageInfo page)
            {
                IsVisibleAsync = page.IsAccessAllowedAsync;
                GetTitle = page.GetDefaultTitle;
                GetUrl = urlHelper => urlHelper.RouteUrl(page.RouteName);
                IsActive = (_, currentPage) => currentPage == page;
            }

            public Func<IUrlHelper, string> GetUrl { get; set; } = null!;
            public Func<HttpContext, PageInfo?, bool> IsActive { get; set; } = null!;
        }

        public class NavigationDropDownItem : NavigationItemBase
        {
            private List<NavigationItemBase>? _items;
            public List<NavigationItemBase> Items
            {
                get => _items ??= new List<NavigationItemBase>();
                set => _items = value;
            }

            public bool IsShown(HttpContext context, PageInfo? currentPage)
            {
                return Items.Any(itemBase =>
                    itemBase is NavigationItem item && item.IsActive(context, currentPage) ||
                    itemBase is NavigationDropDownItem dropDownItem && dropDownItem.IsShown(context, currentPage));
            }
        }
    }
}
