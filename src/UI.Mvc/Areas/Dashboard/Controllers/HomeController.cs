using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Core.Infrastructure;
using WebApp.UI.Areas.Dashboard.Models.Home;

namespace WebApp.UI.Areas.Dashboard.Controllers
{
    [Authorize]
    [Area("Dashboard")]
    public class HomeController : Controller
    {
        private readonly IClock _clock;

        public HomeController(IClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public IActionResult Index()
        {
            var model = new HomeIndexModel();

            var now = _clock.UtcNow.ToLocalTime();
            var midnight = now.Date + TimeSpan.FromDays(1);

            model.TimeToMidnight = midnight - now;

            ViewData["ActiveMenuItem"] = "Dashboard";
            ViewData["ActiveSubMenuItem"] = "Overview";
            return View(model);
        }
    }
}
