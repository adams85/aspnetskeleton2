using WebApp.UI.Areas.Dashboard.Controllers;

namespace WebApp.UI.Areas.Dashboard
{
    public static class DashboardRoutes
    {
        public const string AreaName = "Dashboard";

        #region Routes

        public const string OverviewRouteName = AreaName + "." + nameof(HomeController) + "." + nameof(HomeController.Index);

        public const string SettingsRouteName = AreaName + "." + nameof(SettingsController) + "." + nameof(SettingsController.Index);

        public const string AccountSettingsRouteName = AreaName + "." + nameof(AccountController) + "." + nameof(AccountController.Index);

        #endregion
    }
}
