using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WebApp.Service.Infrastructure.Localization;
using WebApp.Service.Settings;
using WebApp.UI.Filters;
using WebApp.UI.Infrastructure.Security;
using WebApp.UI.Models.Account;

namespace WebApp.UI.Controllers
{
    [Authorize]
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

        public IStringLocalizer T { get; set; }

        private async Task<bool> LoginCoreAsync(LoginModel model, CancellationToken cancellationToken)
        {
            if (await _accountManager.ValidateUserAsync(model, cancellationToken))
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

        [HttpGet]
        [AllowAnonymous]
        [AnonymousOnly]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["ActiveMenuItem"] = "Login";
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [AnonymousOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (ModelState.IsValid)
            {
                if (await LoginCoreAsync(model, cancellationToken))
                    return RedirectToLocal(returnUrl);

                // If we got this far, something failed, redisplay form
                ModelState.AddModelError("", T["Incorrect e-mail address or password."]);
            }

            // If we got this far, something failed, redisplay form
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["ActiveMenuItem"] = "Login";
            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction(nameof(HomeController.Index), "Home", new { area = "" });
        }

        private Task<CreateUserResult> RegisterCoreAsync(RegisterModel model, CancellationToken cancellationToken)
        {
            return _accountManager.CreateUserAsync(model, cancellationToken);
        }

        [HttpGet]
        [AllowAnonymous]
        [AnonymousOnly]
        public IActionResult Register()
        {
            ViewData["ActiveMenuItem"] = "Register";
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [AnonymousOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model, CancellationToken cancellationToken)
        {
            if (!_settingsProvider.EnableRegistration())
                return NotFound();

            var createStatus = CreateUserResult.Success;
            if (ModelState.IsValid &&
                (createStatus = await RegisterCoreAsync(model, cancellationToken)) == CreateUserResult.Success)
            {
                return RedirectToAction(nameof(Verify));
            }
            else
            {
                if (createStatus != CreateUserResult.Success)
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));

                ViewData["ActiveMenuItem"] = "Register";
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [AnonymousOnly]
        public async Task<IActionResult> Verify(string u, string v, CancellationToken cancellationToken)
        {
            bool? model;

            if (u != null && v != null)
                model = await _accountManager.VerifyUserAsync(u, v, cancellationToken);
            else
                model = null;

            ViewData["ActiveMenuItem"] = "Verification";
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [AnonymousOnly]
        public IActionResult ResetPassword(string s)
        {
            var model = new ResetPasswordModel();
            if (s != null)
                model.Success = Convert.ToBoolean(int.Parse(s));

            ViewData["ActiveMenuItem"] = "Password Reset";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [AnonymousOnly]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var success = await _accountManager.ResetPasswordAsync(model, cancellationToken);
                return RedirectToAction(null, new { s = Convert.ToInt32(success) });
            }

            ViewData["ActiveMenuItem"] = "Password Reset";
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [AnonymousOnly]
        public IActionResult SetPassword(string s, string u, string v)
        {
            var model = new SetPasswordModel();
            if (s != null)
                model.Success = Convert.ToBoolean(int.Parse(s));

            ViewData["ActiveMenuItem"] = "New Password";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [AnonymousOnly]
        public async Task<IActionResult> SetPassword(SetPasswordModel model, string u, string v, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var success = await _accountManager.SetPasswordAsync(u, v, model, cancellationToken);
                return RedirectToAction(null, new { s = Convert.ToInt32(success) });
            }

            ViewData["ActiveMenuItem"] = "New Password";
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Helpers
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(HomeController.Index), "Home", new { area = "Dashboard" });
        }

        private string ErrorCodeToString(CreateUserResult result)
        {
            switch (result)
            {
                case CreateUserResult.DuplicateUserName:
                case CreateUserResult.DuplicateEmail:
                    return T["The e-mail address specified is already linked to an existing account."];

                case CreateUserResult.InvalidPassword:
                    return T["The password specified is not formatted correctly. Please enter a valid password value."];

                case CreateUserResult.InvalidEmail:
                    return T["The e-mail address specified is not formatted correctly. Please enter a valid e-mail address."];

                default:
                    return T["An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator."];
            }
        }
        #endregion
    }
}
