using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApp.UI.Areas.Dashboard;
using WebApp.UI.Models.Home;

namespace WebApp.UI.Controllers
{
    [Route("[controller]/[action]")]
    public class HomeController : Controller
    {
        [Route("/")]
        public IActionResult Index()
        {
            return RedirectToRoute(DashboardRoutes.OverviewRouteName);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
