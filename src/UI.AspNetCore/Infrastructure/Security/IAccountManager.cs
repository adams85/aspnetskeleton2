using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WebApp.Api.Infrastructure.Security;
using WebApp.Service.Infrastructure.Validation;
using WebApp.Service.Users;
using WebApp.UI.Areas.Dashboard.Models.Account;
using WebApp.UI.Models.Account;

namespace WebApp.UI.Infrastructure.Security
{
    public interface IAccountManager : ICachedUserInfoProvider
    {
        Task<AuthenticateUserStatus> ValidateUserAsync(NetworkCredential credentials, CancellationToken cancellationToken);
        Task<(CreateUserStatus, PasswordRequirementsData?)> CreateUserAsync(RegisterModel model, CancellationToken cancellationToken);
        Task<(ChangePasswordStatus, PasswordRequirementsData?)> ChangePasswordAsync(string userName, ChangePasswordModel model, CancellationToken cancellationToken);
        Task<bool> VerifyUserAsync(string userName, string verificationToken, CancellationToken cancellationToken);
        Task<bool> ResetPasswordAsync(ResetPasswordModel model, CancellationToken cancellationToken);
        Task<(ChangePasswordStatus, PasswordRequirementsData?)> SetPasswordAsync(string userName, string verificationToken, SetPasswordModel model, CancellationToken cancellationToken);
    }
}
