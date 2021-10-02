using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using WebApp.Common.Infrastructure.Localization;

namespace WebApp.UI.Models.Account
{
    public class LoginModel
    {
        [Localized] private const string UserNameDisplayName = "E-mail address";
        [DisplayName(UserNameDisplayName), Required]
        public string UserName
        {
            get => Credentials.UserName;
            set => Credentials.UserName = value;
        }

        [Localized] private const string PasswordDisplayName = "Password";
        [DisplayName(PasswordDisplayName), Required, DataType(DataType.Password)]
        public string Password
        {
            get => Credentials.Password;
            set => Credentials.Password = value;
        }

        [Localized] private const string RememberMeDisplayName = "Remember me?";
        [DisplayName(RememberMeDisplayName)]
        public bool RememberMe { get; set; }

        [BindNever, ValidateNever]
        public NetworkCredential Credentials { get; } = new NetworkCredential();
    }
}
