using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebApp.Common.Infrastructure.Localization;
using WebApp.Service.Users;

namespace WebApp.UI.Models.Account
{
    public class LoginModel
    {
        [Localized] private const string UserNameDisplayName = "E-mail address";
        [DisplayName(UserNameDisplayName), Required]
        public string UserName { get; set; } = null!;

        [Localized] private const string PasswordDisplayName = "Password";
        [DisplayName(PasswordDisplayName), Required, DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Localized] private const string RememberMeDisplayName = "Remember me?";
        [DisplayName(RememberMeDisplayName)]
        public bool RememberMe { get; set; }

        public AuthenticateUserQuery ToQuery() => new AuthenticateUserQuery()
        {
            UserName = this.UserName,
            Password = this.Password
        };
    }
}
