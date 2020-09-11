using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApp.UI.Helpers;
using WebApp.UI.Models.Home;

namespace WebApp.UI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction(
                nameof(Areas.Dashboard.Controllers.HomeController.Index),
                MvcHelper.GetControllerName<Areas.Dashboard.Controllers.HomeController>(),
                new { area = UIConstants.DashboardAreaName, id = (object?)null });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
