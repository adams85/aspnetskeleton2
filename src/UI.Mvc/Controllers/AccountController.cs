using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using WebApp.Service.Infrastructure.Localization;
using WebApp.Service.Infrastructure.Validation;
using WebApp.Service.Settings;
using WebApp.Service.Users;
using WebApp.UI.Filters;
using WebApp.UI.Infrastructure.Localization;
using WebApp.UI.Infrastructure.Security;
using WebApp.UI.Models;
using WebApp.UI.Models.Account;

namespace WebApp.UI.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly IAccountManager _accountManager;
        private readonly ISettingsProvider _settingsProvider;

        public AccountController(IAccountManager accountManager, ISettingsProvider settingsProvider, IStringLocalizer<AccountController>? stringLocalizer)
        {
            _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
            T = stringLocalizer ?? (IStringLocalizer)NullStringLocalizer.Instance;
        }

        public IStringLocalizer T { get; }

        private async Task<bool> LoginCoreAsync(LoginModel model, CancellationToken cancellationToken)
        {
            var status = await _accountManager.ValidateUserAsync(model.Credentials, cancellationToken);

            if (status == AuthenticateUserStatus.Successful)
            {
                var claims = new Claim[]
                {
                    new Claim(ClaimTypes.Name, model.UserName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return true;
            }
            else
                return false;
        }

        [AllowAnonymous]
        [AnonymousOnly]
        [HttpGet(Name = Routes.LoginRouteName)]
        public IActionResult Login(string? returnUrl = null)
        {
            var model = new LoginModel { ReturnUrl = returnUrl };

            return View(model);
        }

        [AllowAnonymous]
        [AnonymousOnly]
        [ValidateAntiForgeryToken]
        [HttpPost(Name = Routes.LoginRouteName)]
        public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                if (await LoginCoreAsync(model, HttpContext.RequestAborted))
                    return RedirectToLocal(returnUrl);

                // If we got this far, something failed, redisplay form
                ModelState.AddModelError("", T["Incorrect e-mail address or password."]);
            }

            model.ReturnUrl = returnUrl;

            return View(model);
        }

        [AllowAnonymous]
        [Route("", Name = Routes.LogoutRouteName)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction(nameof(HomeController.Index), "Home", new { area = "" });
        }

        private Task<(CreateUserStatus, PasswordRequirementsData?)> RegisterCoreAsync(RegisterModel model, CancellationToken cancellationToken)
        {
            return _accountManager.CreateUserAsync(model, cancellationToken);
        }

        [AllowAnonymous]
        [AnonymousOnly]
        [HttpGet(Name = Routes.RegisterRouteName)]
        public IActionResult Register()
        {
            if (!_settingsProvider.EnableRegistration())
                return NotFound();

            return View(new RegisterModel());
        }

        [AllowAnonymous]
        [AnonymousOnly]
        [ValidateAntiForgeryToken]
        [HttpPost(Name = Routes.RegisterRouteName)]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!_settingsProvider.EnableRegistration())
                return NotFound();

            if (ModelState.IsValid)
            {
                var (status, passwordRequirements) = await RegisterCoreAsync(model, HttpContext.RequestAborted);

                if (status == CreateUserStatus.Success)
                    return RedirectToAction(nameof(Verify));

                AddModelError(ModelState, status, passwordRequirements);
            }

            return View(model);

            void AddModelError(ModelStateDictionary modelState, CreateUserStatus status, PasswordRequirementsData? passwordRequirements)
            {
                switch (status)
                {
                    case CreateUserStatus.DuplicateUserName:
                    case CreateUserStatus.DuplicateEmail:
                        modelState.AddModelError(nameof(RegisterModel.UserName), T["The e-mail address is already linked to an existing account."]);
                        return;

                    case CreateUserStatus.InvalidPassword:
                        modelState.AddModelError(nameof(RegisterModel.Password), T.LocalizePasswordRequirements(passwordRequirements));
                        return;

                    default:
                        modelState.AddModelError(string.Empty, T["An unexpected error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator."]);
                        return;
                }
            }
        }

        [AllowAnonymous]
        [AnonymousOnly]
        [HttpGet(Name = Routes.VerifyRegistrationRouteName)]
        public async Task<IActionResult> Verify(string u, string v)
        {
            var model = new PopupPageModel<bool?>();

            if (u != null && v != null)
                model.Content = await _accountManager.VerifyUserAsync(u, v, HttpContext.RequestAborted);
            else
                model.Content = null;

            return View(model);
        }

        [AllowAnonymous]
        [AnonymousOnly]
        [HttpGet(Name = Routes.ResetPasswordRouteName)]
        public IActionResult ResetPassword(string s)
        {
            var model = new ResetPasswordModel();
            if (s != null)
                model.Success = Convert.ToBoolean(int.Parse(s));

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [AnonymousOnly]
        [HttpPost(Name = Routes.ResetPasswordRouteName)]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _accountManager.ResetPasswordAsync(model, HttpContext.RequestAborted);
                return RedirectToAction(null, new { s = Convert.ToInt32(success) });
            }

            return View(model);
        }

        [AllowAnonymous]
        [AnonymousOnly]
        [HttpGet(Name = Routes.SetPasswordRouteName)]
        public IActionResult SetPassword(string s, string u, string v)
        {
            var model = new SetPasswordModel();
            if (s != null)
                model.Success = Convert.ToBoolean(int.Parse(s));

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [AnonymousOnly]
        [HttpPost(Name = Routes.SetPasswordRouteName)]
        public async Task<IActionResult> SetPassword(SetPasswordModel model, string u, string v)
        {
            if (ModelState.IsValid)
            {
                var (status, passwordRequirements) = await _accountManager.SetPasswordAsync(u, v, model, HttpContext.RequestAborted);

                if (status != ChangePasswordStatus.InvalidNewPassword)
                    return RedirectToAction(null, new { s = Convert.ToInt32(status == ChangePasswordStatus.Success) });

                AddModelError(status, passwordRequirements);
            }

            return View(model);

            void AddModelError(ChangePasswordStatus status, PasswordRequirementsData? passwordRequirements)
            {
                switch (status)
                {
                    case ChangePasswordStatus.InvalidNewPassword:
                        ModelState.AddModelError(nameof(SetPasswordModel.NewPassword), T.LocalizePasswordRequirements(passwordRequirements));
                        return;
                }
            }
        }

        [AllowAnonymous]
        [Route("", Name = Routes.AccessDeniedRouteName)]
        public IActionResult AccessDenied()
        {
            return View(new PopupPageModel());
        }

        #region Helpers

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(HomeController.Index), "Home", new { area = "Dashboard" });
        }

        #endregion
    }
}
