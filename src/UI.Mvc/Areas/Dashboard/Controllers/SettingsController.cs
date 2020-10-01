using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Service.Settings;
using WebApp.UI.Areas.Dashboard.Models;

namespace WebApp.UI.Areas.Dashboard.Controllers
{
    [Authorize]
    [Area(DashboardRoutes.AreaName)]
    [Route("[area]/[controller]/[action]")]
    public class SettingsController : Controller
    {
        [HttpGet("/[area]/[controller]", Name = DashboardRoutes.SettingsRouteName)]
        public IActionResult Index(ListSettingsQuery query)
        {
            var model = new SingleValueDashboardPageModel<ListSettingsQuery>
            {
                Value = query
            };

            return View(model);
        }
    }
}
