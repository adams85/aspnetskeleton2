using System.Collections.Generic;
using System.Threading.Tasks;
using WebApp.Common.Roles;
using WebApp.UI.Infrastructure.Navigation;

namespace WebApp.UI.Areas.Dashboard
{
    public sealed class DashboardPages : IPageCollectionProvider
    {
        private static readonly IReadOnlyList<PageInfo> s_pages = new[]
        {
            new PageInfo(DashboardRoutes.OverviewRouteName, DashboardRoutes.AreaName)
            {
                GetDefaultTitle = (_, t) => t["Overview"]
            },
            new PageInfo(DashboardRoutes.AccountSettingsRouteName, DashboardRoutes.AreaName)
            {
                GetDefaultTitle = (_, t) => t["Account Settings"]
            },
            new PageInfo(DashboardRoutes.SettingsRouteName, DashboardRoutes.AreaName)
            {
                GetDefaultTitle = (_, t) => t["Application Settings"],
                IsAccessAllowedAsync = httpContext => Task.FromResult(httpContext.User.IsInRole(nameof(RoleEnum.Administators)))
            },
        };

        public IEnumerable<PageInfo> GetPages() => s_pages;
    }
}
