using WebApp.UI.Models.Layout;

namespace WebApp.UI.Areas.Dashboard.Models.Layout
{
    public class DashboardPageLayoutModel : LayoutModel
    {
        private DashboardSidebarModel? _sidebar;
        public DashboardSidebarModel Sidebar
        {
            get => _sidebar ??= new DashboardSidebarModel();
            init => _sidebar = value;
        }

        private DashboardHeaderModel? _header;
        public DashboardHeaderModel Header
        {
            get => _header ??= new DashboardHeaderModel();
            init => _header = value;
        }

        private DashboardFooterModel? _footer;
        public DashboardFooterModel Footer
        {
            get => _footer ??= new DashboardFooterModel();
            init => _footer = value;
        }
    }
}
