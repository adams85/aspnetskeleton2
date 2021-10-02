using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.UI.Models;

namespace WebApp.UI.Pages.Account
{
    public class LogoutModel : BasePageModel<LogoutModel.PageDescriptorClass>
    {
        private async Task<IActionResult> LogoutAsync()
        {
            await HttpContext.SignOutAsync();

            return RedirectToPage(IndexModel.PageDescriptor.PageName, new { area = IndexModel.PageDescriptor.AreaName });
        }

        public Task<IActionResult> OnGet() => LogoutAsync();

        public Task<IActionResult> OnPost() => LogoutAsync();

        public sealed class PageDescriptorClass : PageDescriptor
        {
            public override string PageName => "/Account/Logout";
            public override Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetDefaultTitle => throw new NotSupportedException();
        }
    }
}
