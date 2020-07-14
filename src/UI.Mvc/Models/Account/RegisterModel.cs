using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebApp.Common;
using WebApp.Common.Infrastructure.Localization;
using WebApp.Service.Users;

namespace WebApp.UI.Models.Account
{
    public class RegisterModel
    {
        [Localized] private const string UserNameDisplayName = "E-mail address";
        [DisplayName(UserNameDisplayName), Required, EmailAddress, MaxLength(ModelConstants.UserNameMaxLength)]
        public string UserName { get; set; } = null!;

        [Localized] private const string PasswordDisplayName = "Password";
        [DisplayName(PasswordDisplayName), Required, DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Localized] private const string ConfirmPasswordDisplayName = "Confirm password";
        [Localized] private const string ConfirmPasswordCompareErrorMessage = "The password and confirmation password must match.";
        [DisplayName(ConfirmPasswordDisplayName), Compare(nameof(Password), ErrorMessage = ConfirmPasswordCompareErrorMessage), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;

        [Localized] private const string FirstNameDisplayName = "First name";
        [DisplayName(FirstNameDisplayName), Required, MaxLength(ModelConstants.UserFirstNameMaxLength)]
        public string FirstName { get; set; } = null!;

        [Localized] private const string LastNameDisplayName = "Last name";
        [DisplayName(LastNameDisplayName), Required, MaxLength(ModelConstants.UserLastNameMaxLength)]
        public string LastName { get; set; } = null!;

        public CreateUserCommand ToCommand() => new CreateUserCommand()
        {
            UserName = this.UserName,
            Email = this.UserName,
            Password = this.Password,
            FirstName = this.FirstName.Trim(),
            LastName = this.LastName.Trim(),
            IsApproved = false,
            CreateProfile = true,
        };
    }
}
