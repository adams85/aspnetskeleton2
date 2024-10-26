using Microsoft.AspNetCore.Mvc;
using WebApp.UI.Areas.Dashboard.Models.Layout;

namespace WebApp.UI.Areas.Dashboard.ViewComponents.Layout;

public class DashboardSidebarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(DashboardPageLayoutModel model)
    {
        return View(model);
    }
}
