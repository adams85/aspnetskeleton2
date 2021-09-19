using WebApp.UI.Controllers;

namespace WebApp.UI
{
    public static class Routes
    {
        public const string AccessDeniedRouteName = nameof(AccountController) + "." + nameof(AccountController.AccessDenied);
        public const string LoginRouteName = nameof(AccountController) + "." + nameof(AccountController.Login);
        public const string LogoutRouteName = nameof(AccountController) + "." + nameof(AccountController.Logout);
        public const string RegisterRouteName = nameof(AccountController) + "." + nameof(AccountController.Register);
        public const string ResetPasswordRouteName = nameof(AccountController) + "." + nameof(AccountController.ResetPassword);
        public const string SetPasswordRouteName = nameof(AccountController) + "." + nameof(AccountController.SetPassword);
        public const string VerifyRegistrationRouteName = nameof(AccountController) + "." + nameof(AccountController.Verify);
    }
}
