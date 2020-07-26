using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebApp.Common.Infrastructure.Localization;
using WebApp.Service.Users;

namespace WebApp.UI.Models.Account
{
    public class ResetPasswordModel
    {
        [Localized] private const string UserNameDisplayName = "E-mail address";
        [DisplayName(UserNameDisplayName), Required]
        public string UserName { get; set; } = null!;

        [BindNever]
        public bool? Success { get; set; }

        public ResetPasswordCommand ToCommand(TimeSpan tokenExpirationTime) => new ResetPasswordCommand()
        {
            UserName = this.UserName,
            TokenExpirationTimeSpan = tokenExpirationTime
        };
    }
}
