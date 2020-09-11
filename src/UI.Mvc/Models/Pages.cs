using System.Collections.Generic;
using System.Linq;
using Karambolo.Common;
using WebApp.UI.Controllers;
using WebApp.UI.Helpers;

namespace WebApp.UI.Models
{
    public sealed class Pages : PagesBase
    {
        public static readonly Pages PageHelper = new Pages();

        private static readonly Dictionary<(string, string, string), PageInfo> s_pagesByRoute;

        #region Pages

        public static readonly PageInfo AccessDeniedPage;
        public static readonly PageInfo LoginPage;
        public static readonly PageInfo RegisterPage;
        public static readonly PageInfo ResetPasswordPage;
        public static readonly PageInfo SetPasswordPage;
        public static readonly PageInfo VerifyRegistrationPage;

        #endregion

        static Pages()
        {
            s_pagesByRoute = new[]
            {
#pragma warning disable IDE1006 // Naming Styles

                AccessDeniedPage = new PageInfo(nameof(AccountController.AccessDenied), MvcHelper.GetControllerName<AccountController>())
                {
                    GetDefaultTitle = (_, T) => T["Access Denied"]
                },
                LoginPage = new PageInfo(nameof(AccountController.Login), MvcHelper.GetControllerName<AccountController>())
                {
                    GetDefaultTitle = (_, T) => T["Login"]
                },
                RegisterPage = new PageInfo(nameof(AccountController.Register), MvcHelper.GetControllerName<AccountController>())
                {
                    GetDefaultTitle = (_, T) => T["Create an Account"]
                },
                ResetPasswordPage = new PageInfo(nameof(AccountController.ResetPassword), MvcHelper.GetControllerName<AccountController>())
                {
                    GetDefaultTitle = (_, T) => T["Forgotten Password"]
                },
                SetPasswordPage = new PageInfo(nameof(AccountController.SetPassword), MvcHelper.GetControllerName<AccountController>())
                {
                    GetDefaultTitle = (_, T) => T["New Password"]
                },
                VerifyRegistrationPage = new PageInfo(nameof(AccountController.Verify), MvcHelper.GetControllerName<AccountController>())
                {
                    GetDefaultTitle = (_, T) => T["Account Verification"]
                }

#pragma warning restore IDE1006 // Naming Styles
            }
            .ToDictionary(page => page.RouteValues, Identity<PageInfo>.Func);
        }

        private Pages() { }

        protected override PageInfo? GetPageByRouteCore((string, string, string) routeValues) =>
            s_pagesByRoute.TryGetValue(routeValues, out var pageInfo) ? pageInfo : null;
    }
}
