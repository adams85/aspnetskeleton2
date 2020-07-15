using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using WebApp.Service.Infrastructure.Localization;
using WebApp.Service.Infrastructure.Validation;
using WebApp.UI.Areas.Dashboard.Models.Account;
using WebApp.UI.Infrastructure.Localization;
using WebApp.UI.Infrastructure.Security;

namespace WebApp.UI.Areas.Dashboard.Controllers
{
    [Authorize]
    [Area("Dashboard")]
    public class AccountController : Controller
    {
        private readonly IAccountManager _accountManager;

        public AccountController(IAccountManager accountManager, IStringLocalizer<AccountController>? stringLocalizer)
        {
            _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
            T = stringLocalizer ?? (IStringLocalizer)NullStringLocalizer.Instance;
        }

        public IStringLocalizer T { get; set; }

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
            bool success;

            if (ModelState.IsValid)
            {
                var (status, passwordRequirements) = await _accountManager.ChangePasswordAsync(HttpContext.User.Identity.Name!, model, cancellationToken);

                if (status == ChangePasswordStatus.InvalidNewPassword)
                    AddModelError(ModelState, status, passwordRequirements);

                success = status == ChangePasswordStatus.Success;
            }
            else
                success = false;

            model.Success = success;

            ViewData["ActiveMenuItem"] = "Dashboard";
            ViewData["ActiveSubMenuItem"] = "Account";
            return View(model);

            void AddModelError(ModelStateDictionary modelState, ChangePasswordStatus status, PasswordRequirementsData? passwordRequirements)
            {
                switch (status)
                {
                    case ChangePasswordStatus.InvalidNewPassword:
                        modelState.AddModelError(nameof(ChangePasswordModel.NewPassword), T.LocalizePasswordRequirements(passwordRequirements));
                        return;
                }
            }
        }
    }
}
