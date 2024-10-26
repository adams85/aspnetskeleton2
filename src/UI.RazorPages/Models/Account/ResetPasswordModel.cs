using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebApp.Common.Infrastructure.Localization;
using WebApp.Service.Users;

namespace WebApp.UI.Models.Account;

public record class ResetPasswordModel
{
    [Localized] private const string UserNameDisplayName = "E-mail address";
    [DisplayName(UserNameDisplayName), Required]
    public string UserName { get; init; } = null!;

    public ResetPasswordCommand ToCommand(TimeSpan tokenExpirationTime) => new ResetPasswordCommand()
    {
        UserName = this.UserName,
        TokenExpirationTimeSpan = tokenExpirationTime
    };
}
