using System.Collections.Generic;
using WebApp.UI.Infrastructure.Navigation;

namespace WebApp.UI.Areas.Dashboard
{
    public sealed class DashboardPages : IPageCollectionProvider
    {
        private static readonly IReadOnlyList<PageInfo> s_pages = new[]
        {
#pragma warning disable IDE1006 // Naming Styles

            new PageInfo(DashboardRoutes.OverviewRouteName, DashboardRoutes.AreaName)
            {
                GetDefaultTitle = (_, T) => T["Overview"]
            },
            new PageInfo(DashboardRoutes.AccountSettingsRouteName, DashboardRoutes.AreaName)
            {
                GetDefaultTitle = (_, T) => T["Account Settings"]
            },

#pragma warning restore IDE1006 // Naming Styles
        };

        public IEnumerable<PageInfo> GetPages() => s_pages;
    }
}
