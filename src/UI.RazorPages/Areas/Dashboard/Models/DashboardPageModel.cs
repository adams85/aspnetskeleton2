using WebApp.UI.Areas.Dashboard.Models.Layout;
using WebApp.UI.Models;
using WebApp.UI.Models.Layout;

namespace WebApp.UI.Areas.Dashboard.Models;

public abstract class DashboardPageModel<TPageDescriptor> : BasePageModel<TPageDescriptor>, ILayoutModelProvider<DashboardPageLayoutModel>
    where TPageDescriptor : PageDescriptor, new()
{
    public DashboardPageLayoutModel Layout
    {
        get => field ??= new DashboardPageLayoutModel();
        set;
    }
}
