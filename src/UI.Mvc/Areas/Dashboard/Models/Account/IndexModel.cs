using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApp.UI.Areas.Dashboard.Models.Account
{
    public class IndexModel : DashboardPageModel
    {
        public string? SubmitAction { get; set; }

        private ChangePasswordModel? _changePassword;
        [BindNever]
        public ChangePasswordModel ChangePassword
        {
            get => _changePassword ??= new ChangePasswordModel();
            set => _changePassword = value;
        }
    }
}
