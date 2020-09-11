using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [Area(UIConstants.DashboardAreaName)]
    public class AccountController : Controller
    {
        private readonly IAccountManager _accountManager;

        public AccountController(IAccountManager accountManager, IStringLocalizer<AccountController>? stringLocalizer)
        {
            _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
            T = stringLocalizer ?? (IStringLocalizer)NullStringLocalizer.Instance;
        }

        public IStringLocalizer T { get; set; }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new IndexModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IndexModel model)
        {
            ModelState.Clear();

            return model.SubmitAction switch
            {
                nameof(ChangePassword) => await ChangePassword(model),
                _ => BadRequest()
            };
        }

        [NonAction]
        public async Task<IActionResult> ChangePassword(IndexModel model)
        {
            const string prefix = nameof(model.ChangePassword);

            await TryUpdateModelAsync(model.ChangePassword, prefix);

            bool? success;

            if (ModelState.IsValid)
            {
                var (status, passwordRequirements) = await _accountManager.ChangePasswordAsync(HttpContext.User.Identity.Name!, model.ChangePassword!, HttpContext.RequestAborted);

                if (status == ChangePasswordStatus.InvalidNewPassword)
                    AddModelError(status, passwordRequirements);

                success = status == ChangePasswordStatus.Success;
            }
            else
                success = null;

            model.ChangePassword!.Success = success;

            return View(nameof(Index), model);

            void AddModelError(ChangePasswordStatus status, PasswordRequirementsData? passwordRequirements)
            {
                switch (status)
                {
                    case ChangePasswordStatus.InvalidNewPassword:
                        ModelState.AddModelError(prefix + "." + nameof(ChangePasswordModel.NewPassword), T.LocalizePasswordRequirements(passwordRequirements));
                        return;
                }
            }
        }
    }
}
