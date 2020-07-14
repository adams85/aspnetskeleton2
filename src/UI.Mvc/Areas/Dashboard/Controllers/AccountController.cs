using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.UI.Areas.Dashboard.Models.Account;
using WebApp.UI.Infrastructure.Security;

namespace WebApp.UI.Areas.Dashboard.Controllers
{
    [Authorize]
    [Area("Dashboard")]
    public class AccountController : Controller
    {
        private readonly IAccountManager _accountManager;

        public AccountController(IAccountManager accountManager)
        {
            _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
        }

        public IActionResult Index()
        {
            var model = new ChangePasswordModel();

            ViewData["ActiveMenuItem"] = "Dashboard";
            ViewData["ActiveSubMenuItem"] = "Account";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ChangePasswordModel model, CancellationToken cancellationToken)
        {
            model.Success = ModelState.IsValid && await _accountManager.ChangePasswordAsync(HttpContext.User.Identity.Name!, model, cancellationToken);

            ViewData["ActiveMenuItem"] = "Dashboard";
            ViewData["ActiveSubMenuItem"] = "Account";
            return View(model);
        }
    }
}
