using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using WebApp.Service.Infrastructure.Localization;
using WebApp.Service.Infrastructure.Validation;
using WebApp.UI.Areas.Dashboard.Models;
using WebApp.UI.Areas.Dashboard.Models.Account;
using WebApp.UI.Infrastructure.Localization;
using WebApp.UI.Infrastructure.Security;
using WebApp.UI.Models;

namespace WebApp.UI.Areas.Dashboard.Pages.Account
{
    [Authorize]
    public class IndexModel : DashboardPageModel<IndexModel.PageDescriptorClass>
    {
        public const string ChangePasswordHandler = "ChangePassword";

        private readonly IStringLocalizer _t;

        public IndexModel(IStringLocalizer<IndexModel>? stringLocalizer)
        {
            _t = stringLocalizer ?? (IStringLocalizer)NullStringLocalizer.Instance;
        }

        private ChangePasswordModel? _changePasswordModel;
        [BindProperty]
        public ChangePasswordModel ChangePasswordModel
        {
            get => _changePasswordModel ??= new ChangePasswordModel();
            set => _changePasswordModel = value;
        }

        public bool? ChangePasswordSuccess { get; set; }

        public void OnGet()
        {
        }

        public void OnGetChangePassword()
        {
            ChangePasswordSuccess = true;
        }

        public async Task<IActionResult> OnPostChangePassword([FromServices] IAccountManager accountManager)
        {
            if (ModelState.IsValid)
            {
                var (status, passwordRequirements) = await accountManager.ChangePasswordAsync(HttpContext.User.Identity.Name!, ChangePasswordModel, HttpContext.RequestAborted);

                if (status == ChangePasswordStatus.Success)
                    return RedirectToPage(null, ChangePasswordHandler);

                AddModelError(status, passwordRequirements);
            }

            ChangePasswordSuccess = false;

            return Page();

            void AddModelError(ChangePasswordStatus status, PasswordRequirementsData? passwordRequirements)
            {
                switch (status)
                {
                    case ChangePasswordStatus.InvalidNewPassword:
                        ModelState.AddModelError(nameof(ChangePasswordModel) + "." + nameof(Models.Account.ChangePasswordModel.NewPassword), _t.LocalizePasswordRequirements(passwordRequirements));
                        return;
                    default:
                        ModelState[nameof(ChangePasswordModel) + "." + nameof(Models.Account.ChangePasswordModel.CurrentPassword)].ValidationState = ModelValidationState.Invalid;
                        return;
                }
            }
        }

        public sealed class PageDescriptorClass : PageDescriptor
        {
            public override string PageName => "/Account/Index";
            public override string AreaName => DashboardConstants.AreaName;
            public override Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetDefaultTitle { get; } = (_, t) => t["Account Settings"];
        }
    }
}
