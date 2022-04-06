using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using WebApp.Service.Infrastructure.Localization;
using WebApp.Service.Users;
using WebApp.UI.Infrastructure.Security;
using WebApp.UI.Models;

namespace WebApp.UI.Pages.Account
{
    [AnonymousOnly]
    public class LoginModel : CardPageModel<LoginModel.PageDescriptorClass>
    {
        private readonly IAccountManager _accountManager;
        private readonly IStringLocalizer _t;

        public LoginModel(IAccountManager accountManager, IStringLocalizer<LoginModel>? stringLocalizer)
        {
            _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
            _t = stringLocalizer ?? (IStringLocalizer)NullStringLocalizer.Instance;
        }

        private Models.Account.LoginModel? _model;
        [BindProperty]
        public Models.Account.LoginModel Model
        {
            get => _model ??= new Models.Account.LoginModel();
            set => _model = value;
        }

        public string? ReturnUrl { get; set; }

        private async Task<bool> LoginAsync(CancellationToken cancellationToken)
        {
            var status = await _accountManager.ValidateUserAsync(Model.Credentials, cancellationToken);

            if (status != AuthenticateUserStatus.Successful)
                return false;

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, Model.UserName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = Model.RememberMe
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return true;
        }

        public void OnGet([FromQuery] string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPost([FromQuery] string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                if (await LoginAsync(HttpContext.RequestAborted))
                    return RedirectToLocal(returnUrl);

                // If we got this far, something failed, redisplay form
                ModelState.AddModelError(string.Empty, _t["Incorrect e-mail address or password."]);
            }

            ReturnUrl = returnUrl;

            return Page();

            IActionResult RedirectToLocal(string? returnUrl)
            {
                if (Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToPage(Areas.Dashboard.Pages.IndexModel.PageDescriptor.PageName, new { area = Areas.Dashboard.Pages.IndexModel.PageDescriptor.AreaName });
            }
        }

        public sealed class PageDescriptorClass : PageDescriptor
        {
            public override string PageName => "/Account/Login";

            public override LocalizedHtmlString GetDefaultTitle(HttpContext httpContext, IHtmlLocalizer t) => t["Login"];
        }
    }
}
