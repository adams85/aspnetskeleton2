using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebApp.Common.Infrastructure.Localization;
using WebApp.Service.Users;

namespace WebApp.UI.Areas.Dashboard.Models.Account
{
    public record class ChangePasswordModel
    {
        [Localized] private const string CurrentPasswordDisplayName = "Current password";
        [DisplayName(CurrentPasswordDisplayName), Required, DataType(DataType.Password)]
        public string CurrentPassword { get; init; } = null!;

        [Localized] private const string NewPasswordDisplayName = "New password";
        [DisplayName(NewPasswordDisplayName), Required, DataType(DataType.Password)]
        public string NewPassword { get; init; } = null!;

        [Localized] private const string ConfirmPasswordDisplayName = "Confirm password";
        [Localized] private const string ConfirmPasswordCompareErrorMessage = "The password and confirmation password must match.";
        [DisplayName(ConfirmPasswordDisplayName), Compare(nameof(NewPassword), ErrorMessage = ConfirmPasswordCompareErrorMessage), DataType(DataType.Password)]
        public string ConfirmPassword { get; init; } = null!;

        public ChangePasswordCommand ToCommand(string userName) => new ChangePasswordCommand()
        {
            UserName = userName,
            NewPassword = this.NewPassword,
            Verify = false,
        };
    }
}
