using System.ComponentModel.DataAnnotations;
using WebApp.Common.Infrastructure.Localization;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebApp.Service.Users;
using System;

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
