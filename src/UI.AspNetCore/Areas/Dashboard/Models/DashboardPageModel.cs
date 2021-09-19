using WebApp.UI.Areas.Dashboard.Models.Layout;
using WebApp.UI.Models.Layout;

namespace WebApp.UI.Areas.Dashboard.Models
{
    public class DashboardPageModel : ILayoutModelProvider<DashboardPageLayoutModel>
    {
        private DashboardPageLayoutModel? _layout;
        public DashboardPageLayoutModel Layout
        {
            get => _layout ??= new DashboardPageLayoutModel();
            set => _layout = value;
        }
    }

    public class DashboardPageModel<TContent> : DashboardPageModel
    {
        public TContent Content { get; set; } = default!;
    }
}
