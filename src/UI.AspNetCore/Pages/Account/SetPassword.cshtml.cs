using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using WebApp.Service.Infrastructure.Localization;
using WebApp.Service.Infrastructure.Validation;
using WebApp.UI.Infrastructure.Localization;
using WebApp.UI.Infrastructure.Security;
using WebApp.UI.Models;

namespace WebApp.UI.Pages.Account
{
    [AnonymousOnly]
    public class SetPasswordModel : CardPageModel<SetPasswordModel.PageDescriptorClass>
    {
        private readonly IAccountManager _accountManager;
        private readonly IStringLocalizer _t;

        public SetPasswordModel(IAccountManager accountManager, IStringLocalizer<SetPasswordModel>? stringLocalizer)
        {
            _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
            _t = stringLocalizer ?? (IStringLocalizer)NullStringLocalizer.Instance;
        }

        private Models.Account.SetPasswordModel? _model;
        [BindProperty]
        public Models.Account.SetPasswordModel Model
        {
            get => _model ??= new Models.Account.SetPasswordModel();
            set => _model = value;
        }

        public bool? Success { get; set; }

        public void OnGet(string s)
        {
            if (s != null)
                Success = Convert.ToBoolean(int.Parse(s, CultureInfo.InvariantCulture));
        }

        public async Task<IActionResult> OnPost([FromQuery] string u, [FromQuery] string v)
        {
            if (ModelState.IsValid)
            {
                var (status, passwordRequirements) = await _accountManager.SetPasswordAsync(u, v, Model, HttpContext.RequestAborted);

                if (status != ChangePasswordStatus.InvalidNewPassword)
                    return RedirectToPage(new { s = Convert.ToInt32(status == ChangePasswordStatus.Success) });

                AddModelError(status, passwordRequirements);
            }

            return Page();

            void AddModelError(ChangePasswordStatus status, PasswordRequirementsData? passwordRequirements)
            {
                switch (status)
                {
                    case ChangePasswordStatus.InvalidNewPassword:
                        ModelState.AddModelError(nameof(Model) + "." + nameof(Models.Account.SetPasswordModel.NewPassword), _t.LocalizePasswordRequirements(passwordRequirements));
                        return;
                }
            }
        }

        public sealed class PageDescriptorClass : PageDescriptor
        {
            public override string PageName => "/Account/SetPassword";
            public override Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetDefaultTitle { get; } = (_, t) => t["New Password"];
        }
    }
}
