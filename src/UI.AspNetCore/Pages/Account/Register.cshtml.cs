using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using WebApp.Service.Infrastructure.Localization;
using WebApp.Service.Infrastructure.Validation;
using WebApp.Service.Settings;
using WebApp.UI.Infrastructure.Localization;
using WebApp.UI.Infrastructure.Security;
using WebApp.UI.Models;

namespace WebApp.UI.Pages.Account;

[AnonymousOnly]
public class RegisterModel : CardPageModel<RegisterModel.PageDescriptorClass>
{
    private readonly IAccountManager _accountManager;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IStringLocalizer _t;

    public RegisterModel(IAccountManager accountManager, ISettingsProvider settingsProvider, IStringLocalizer<RegisterModel>? stringLocalizer)
    {
        _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
        _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
        _t = stringLocalizer ?? (IStringLocalizer)NullStringLocalizer.Instance;
    }

    private Models.Account.RegisterModel? _model;
    [BindProperty]
    public Models.Account.RegisterModel Model
    {
        get => _model ??= new Models.Account.RegisterModel();
        set => _model = value;
    }

    private Task<(CreateUserStatus, PasswordRequirementsData?)> RegisterAsync(CancellationToken cancellationToken)
    {
        return _accountManager.CreateUserAsync(Model, cancellationToken);
    }

    public IActionResult OnGet()
    {
        if (!_settingsProvider.EnableRegistration())
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!_settingsProvider.EnableRegistration())
            return NotFound();

        if (ModelState.IsValid)
        {
            var (status, passwordRequirements) = await RegisterAsync(HttpContext.RequestAborted);

            if (status == CreateUserStatus.Success)
                return RedirectToPage(VerifyModel.PageDescriptor.PageName, new { area = VerifyModel.PageDescriptor.AreaName });

            AddModelError(ModelState, status, passwordRequirements);
        }

        return Page();

        void AddModelError(ModelStateDictionary modelState, CreateUserStatus status, PasswordRequirementsData? passwordRequirements)
        {
            switch (status)
            {
                case CreateUserStatus.DuplicateUserName:
                case CreateUserStatus.DuplicateEmail:
                    modelState.AddModelError(nameof(Model) + "." + nameof(Models.Account.RegisterModel.UserName), _t["The e-mail address is already linked to an existing account."]);
                    return;

                case CreateUserStatus.InvalidPassword:
                    modelState.AddModelError(nameof(Model) + "." + nameof(Models.Account.RegisterModel.Password), _t.LocalizePasswordRequirements(passwordRequirements));
                    return;

                default:
                    modelState.AddModelError(string.Empty, _t["An unexpected error occurred. Please verify your entry and try again. If the problem persists, please contact the system administrator."]);
                    return;
            }
        }
    }

    public sealed class PageDescriptorClass : PageDescriptor
    {
        public override string PageName => "/Account/Register";

        public override LocalizedHtmlString GetDefaultTitle(HttpContext httpContext, IHtmlLocalizer t) => t["Create an Account"];
    }
}
