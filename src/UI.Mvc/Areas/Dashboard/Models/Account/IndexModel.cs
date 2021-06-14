using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebApp.UI.Areas.Dashboard.Models.Account
{
    public class IndexModel : DashboardPageModel
    {
        public string? SubmitAction { get; set; }

        private ChangePasswordModel? _changePassword;
        [BindNever, ValidateNever]
        public ChangePasswordModel ChangePassword
        {
            get => _changePassword ??= new ChangePasswordModel();
            set => _changePassword = value;
        }
    }
}
