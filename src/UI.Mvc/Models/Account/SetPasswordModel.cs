using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebApp.Common.Infrastructure.Localization;
using WebApp.Service.Users;

namespace WebApp.UI.Models.Account
{
    public class SetPasswordModel
    {
        [Localized] private const string NewPasswordDisplayName = "New password";
        [DisplayName(NewPasswordDisplayName), Required, DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;

        [Localized] private const string ConfirmPasswordDisplayName = "Confirm password";
        [Localized] private const string ConfirmPasswordCompareErrorMessage = "The password and confirmation password must match.";
        [DisplayName(ConfirmPasswordDisplayName), Compare(nameof(NewPassword), ErrorMessage = ConfirmPasswordCompareErrorMessage), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;

        [BindNever]
        public bool? Success { get; set; }

        public ChangePasswordCommand ToCommand(string userName, string verificationToken) => new ChangePasswordCommand()
        {
            UserName = userName,
            NewPassword = this.NewPassword,
            Verify = true,
            VerificationToken = verificationToken,
        };
    }
}
