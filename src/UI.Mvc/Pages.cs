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
#pragma warning disable IDE1006 // Naming Styles

            new PageInfo(Routes.AccessDeniedRouteName)
            {
                GetDefaultTitle = (_, T) => T["Access Denied"]
            },
            new PageInfo(Routes.LoginRouteName)
            {
                GetDefaultTitle = (_, T) => T["Login"]
            },
            new PageInfo(Routes.RegisterRouteName)
            {
                GetDefaultTitle = (_, T) => T["Create an Account"]
            },
            new PageInfo(Routes.ResetPasswordRouteName)
            {
                GetDefaultTitle = (_, T) => T["Forgotten Password"]
            },
            new PageInfo(Routes.SetPasswordRouteName)
            {
                GetDefaultTitle = (_, T) => T["New Password"]
            },
            new PageInfo(Routes.VerifyRegistrationRouteName)
            {
                GetDefaultTitle = (_, T) => T["Account Verification"]
            },

#pragma warning restore IDE1006 // Naming Styles
        };

        public IEnumerable<PageInfo> GetPages() => s_pages;
    }
}
