using System.Threading;
using System.Threading.Tasks;
using WebApp.Service.Users;
using WebApp.UI.Areas.Dashboard.Models.Account;
using WebApp.UI.Models.Account;

namespace WebApp.UI.Infrastructure.Security
{
    public interface IAccountManager
    {
        Task<CachedUserInfoData?> GetCachedUserInfo(string userName, bool registerActivity, CancellationToken cancellationToken);

        Task<bool> ValidateUserAsync(LoginModel model, CancellationToken cancellationToken);
        Task<CreateUserResult> CreateUserAsync(RegisterModel model, CancellationToken cancellationToken);
        Task<bool> ChangePasswordAsync(string userName, ChangePasswordModel model, CancellationToken cancellationToken);
        Task<bool> VerifyUserAsync(string userName, string verificationToken, CancellationToken cancellationToken);
        Task<bool> ResetPasswordAsync(ResetPasswordModel model, CancellationToken cancellationToken);
        Task<bool> SetPasswordAsync(string userName, string verificationToken, SetPasswordModel model, CancellationToken cancellationToken);
    }
}
