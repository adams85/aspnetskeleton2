using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.UI.Infrastructure.Security;
using WebApp.UI.Models;

namespace WebApp.UI.Pages.Account
{
    [AnonymousOnly]
    public class VerifyModel : CardPageModel<VerifyModel.PageDescriptorClass>
    {
        private readonly IAccountManager _accountManager;

        public VerifyModel(IAccountManager accountManager)
        {
            _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
        }

        public bool? Success { get; private set; }

        public async Task OnGet([FromQuery] string u, [FromQuery] string v)
        {
            if (u != null && v != null)
                Success = await _accountManager.VerifyUserAsync(u, v, HttpContext.RequestAborted);
            else
                Success = null;
        }

        public sealed class PageDescriptorClass : PageDescriptor
        {
            public override string PageName => "/Account/Verify";

            public override LocalizedHtmlString GetDefaultTitle(HttpContext httpContext, IHtmlLocalizer t) => t["Account Verification"];
        }
    }
}
