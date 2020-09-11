using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Core.Infrastructure;
using WebApp.UI.Areas.Dashboard.Models.Home;

namespace WebApp.UI.Areas.Dashboard.Controllers
{
    [Authorize]
    [Area(UIConstants.DashboardAreaName)]
    public class HomeController : Controller
    {
        private readonly IClock _clock;

        public HomeController(IClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public IActionResult Index()
        {
            var utcNow = _clock.UtcNow;
            var midnight = utcNow.Date + TimeSpan.FromDays(1);

            var model = new IndexModel
            {
                UtcNow = utcNow,
                TimeToMidnight = midnight - utcNow
            };

            return View(model);
        }
    }
}
