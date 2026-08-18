using WebApp.UI.Models.Layout;

namespace WebApp.UI.Areas.Dashboard.Models.Layout;

public class DashboardPageLayoutModel : LayoutModel
{
    public DashboardSidebarModel Sidebar
    {
        get => field ??= new DashboardSidebarModel();
        init;
    }

    public DashboardHeaderModel Header
    {
        get => field ??= new DashboardHeaderModel();
        init;
    }

    public DashboardFooterModel Footer
    {
        get => field ??= new DashboardFooterModel();
        init;
    }
}
