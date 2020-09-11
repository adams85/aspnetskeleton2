using System.Collections.Generic;
using System.Linq;
using Karambolo.Common;
using WebApp.UI.Areas.Dashboard.Controllers;
using WebApp.UI.Helpers;
using WebApp.UI.Models;

namespace WebApp.UI.Areas.Dashboard.Models
{
    public sealed class DashboardPages : PagesBase
    {
        public static readonly DashboardPages DashboardPageHelper = new DashboardPages();

        private static readonly Dictionary<(string, string, string), PageInfo> s_pagesByRoute;

        #region Pages

        public static readonly PageInfo OverviewPage ;

        public static readonly PageInfo AccountSettingsPage;

        #endregion

        static DashboardPages()
        {
            s_pagesByRoute = new[]
            {
#pragma warning disable IDE1006 // Naming Styles

                OverviewPage = new PageInfo(nameof(HomeController.Index), MvcHelper.GetControllerName<HomeController>(), UIConstants.DashboardAreaName)
                {
                    GetDefaultTitle = (_, T) => T["Overview"]
                },

                AccountSettingsPage = new PageInfo(nameof(AccountController.Index), MvcHelper.GetControllerName<AccountController>(), UIConstants.DashboardAreaName)
                {
                    GetDefaultTitle = (_, T) => T["Account Settings"]
                },

#pragma warning restore IDE1006 // Naming Styles
            }
            .ToDictionary(page => page.RouteValues, Identity<PageInfo>.Func);
        }

        private DashboardPages() { }

        protected override PageInfo? GetPageByRouteCore((string, string, string) routeValues) =>
            s_pagesByRoute.TryGetValue(routeValues, out var pageInfo) ? pageInfo : null;
    }
}
