using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.UI.Infrastructure.Security;
using WebApp.UI.Models;

namespace WebApp.UI.Pages.Account;

[AnonymousOnly]
public class ResetPasswordModel : CardPageModel<ResetPasswordModel.PageDescriptorClass>
{
    private readonly IAccountManager _accountManager;

    public ResetPasswordModel(IAccountManager accountManager)
    {
        _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
    }

    [BindProperty]
    public Models.Account.ResetPasswordModel Model
    {
        get => field ??= new Models.Account.ResetPasswordModel();
        set;
    }

    public bool? Success { get; private set; }

    public void OnGet([FromQuery] string s)
    {
        if (s is not null)
            Success = Convert.ToBoolean(int.Parse(s, CultureInfo.InvariantCulture));
    }

    public async Task<IActionResult> OnPost()
    {
        if (ModelState.IsValid)
        {
            var success = await _accountManager.ResetPasswordAsync(Model, HttpContext.RequestAborted);
            return RedirectToPage(new { s = Convert.ToInt32(success) });
        }

        return Page();
    }

    public sealed class PageDescriptorClass : PageDescriptor
    {
        public override string PageName => "/Account/ResetPassword";

        public override LocalizedHtmlString GetDefaultTitle(HttpContext httpContext, IHtmlLocalizer t) => t["Forgotten Password"];
    }
}
