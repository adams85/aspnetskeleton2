using System.Collections.Generic;
using WebApp.UI.Infrastructure.Navigation;

namespace WebApp.UI
{
    public sealed class Pages : IPageCollectionProvider
    {
        public static readonly string EditPopupViewName = "Popups/EditPopup";
        public static readonly string DeletePopupViewName = "Popups/DeletePopup";

        private static readonly IReadOnlyList<PageInfo> s_pages = new[]
        {
            new PageInfo(Routes.AccessDeniedRouteName)
            {
                GetDefaultTitle = (_, t) => t["Access Denied"]
            },
            new PageInfo(Routes.LoginRouteName)
            {
                GetDefaultTitle = (_, t) => t["Login"]
            },
            new PageInfo(Routes.RegisterRouteName)
            {
                GetDefaultTitle = (_, t) => t["Create an Account"]
            },
            new PageInfo(Routes.ResetPasswordRouteName)
            {
                GetDefaultTitle = (_, t) => t["Forgotten Password"]
            },
            new PageInfo(Routes.SetPasswordRouteName)
            {
                GetDefaultTitle = (_, t) => t["New Password"]
            },
            new PageInfo(Routes.VerifyRegistrationRouteName)
            {
                GetDefaultTitle = (_, t) => t["Account Verification"]
            },
        };

        public IEnumerable<PageInfo> GetPages() => s_pages;
    }
}
