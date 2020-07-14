using System;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using Microsoft.Extensions.Options;
using WebApp.Service;
using WebApp.Service.Infrastructure;
using WebApp.Service.Users;
using WebApp.UI.Areas.Dashboard.Models.Account;
using WebApp.UI.Models.Account;

namespace WebApp.UI.Infrastructure.Security
{
    public sealed class AccountManager : IAccountManager
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;

        private readonly TimeSpan _passwordTokenExpirationTime;

        public AccountManager(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher, IOptions<UISecurityOptions>? options)
        {
            _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
            _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));

            var optionsValue = options?.Value;

            _passwordTokenExpirationTime = optionsValue?.PasswordTokenExpirationTime ?? UISecurityOptions.DefaultPasswordTokenExpirationTime;
        }
        
        public async Task<CachedUserInfoData?> GetCachedUserInfo(string userName, bool registerActivity, CancellationToken cancellationToken)
        {
            var result = await _queryDispatcher.DispatchAsync(new GetCachedUserInfoQuery { UserName = userName }, cancellationToken);

            if (result == null)
                return null;

            if (registerActivity)
                await _commandDispatcher.DispatchAsync(new RegisterUserActivityCommand
                {
                    UserName = result.UserName,
                    SuccessfulLogin = null,
                    UIActivity = true,
                }, CancellationToken.None);

            return result;
        }

        public async Task<bool> ValidateUserAsync(LoginModel model, CancellationToken cancellationToken)
        {
            var authResult = await _queryDispatcher.DispatchAsync(model.ToQuery(), cancellationToken);
            if (authResult.UserId == null)
                return false;

            var success = authResult.Status == AuthenticateUserStatus.Successful;

            if (success || authResult.Status == AuthenticateUserStatus.Failed)
                await _commandDispatcher.DispatchAsync(new RegisterUserActivityCommand
                {
                    UserName = model.UserName,
                    SuccessfulLogin = success,
                    UIActivity = success,
                }, CancellationToken.None);

            return success;
        }

        public async Task<CreateUserResult> CreateUserAsync(RegisterModel model, CancellationToken cancellationToken)
        {
            CreateUserResult status;
            try
            {
                await _commandDispatcher.DispatchAsync(model.ToCommand(), cancellationToken);
                status = CreateUserResult.Success;
            }
            catch (ServiceErrorException ex)
            {
                string paramPath;
                switch (ex.ErrorCode)
                {
                    case ServiceErrorCode.ParamNotSpecified:
                    case ServiceErrorCode.ParamNotValid:
                        paramPath = (string)ex.Args[0];
                        status =
                            paramPath == Lambda.MemberPath((CreateUserCommand c) => c.UserName) ? CreateUserResult.InvalidUserName :
                            paramPath == Lambda.MemberPath((CreateUserCommand c) => c.Email) ? CreateUserResult.InvalidEmail :
                            paramPath == Lambda.MemberPath((CreateUserCommand c) => c.Password) ? CreateUserResult.InvalidPassword :
                            CreateUserResult.UnexpectedError;
                        break;
                    case ServiceErrorCode.EntityNotUnique:
                        paramPath = (string)ex.Args[0];
                        status =
                            paramPath == Lambda.MemberPath((CreateUserCommand c) => c.UserName) ? CreateUserResult.DuplicateUserName :
                            paramPath == Lambda.MemberPath((CreateUserCommand c) => c.Email) ? CreateUserResult.DuplicateEmail :
                            CreateUserResult.UnexpectedError;
                        break;
                    default:
                        status = CreateUserResult.UnexpectedError;
                        break;
                }
            }

            return status;
        }

        public async Task<bool> ChangePasswordAsync(string userName, ChangePasswordModel model, CancellationToken cancellationToken)
        {
            var authResult = await _queryDispatcher.DispatchAsync(new AuthenticateUserQuery
            {
                UserName = userName,
                Password = model.CurrentPassword
            }, cancellationToken);

            if (authResult.UserId == null)
                return false;

            var success = authResult.Status == AuthenticateUserStatus.Successful;

            if (success || authResult.Status == AuthenticateUserStatus.Failed)
                await _commandDispatcher.DispatchAsync(new RegisterUserActivityCommand
                {
                    UserName = userName,
                    SuccessfulLogin = success,
                    UIActivity = success,
                }, CancellationToken.None);

            if (success)
                await _commandDispatcher.DispatchAsync(model.ToCommand(userName), cancellationToken);

            return success;
        }

        public async Task<bool> VerifyUserAsync(string userName, string verificationToken, CancellationToken cancellationToken)
        {
            try
            {
                await _commandDispatcher.DispatchAsync(new ApproveUserCommand
                {
                    UserName = userName,
                    Verify = true,
                    VerificationToken = verificationToken,
                }, cancellationToken);

                return true;
            }
            catch (ServiceErrorException)
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordModel model, CancellationToken cancellationToken)
        {
            try
            {
                await _commandDispatcher.DispatchAsync(model.ToCommand(_passwordTokenExpirationTime), cancellationToken);
                return true;
            }
            catch (ServiceErrorException ex)
            {
                // when user doesn't exist, success should be display to the user to prevent probing for existing accounts
                return
                    ex.ErrorCode == ServiceErrorCode.EntityNotFound &&
                    ((string)ex.Args[0]) == Lambda.MemberPath((ResetPasswordCommand c) => c.UserName);
            }
        }

        public async Task<bool> SetPasswordAsync(string userName, string verificationToken, SetPasswordModel model, CancellationToken cancellationToken)
        {
            try
            {
                await _commandDispatcher.DispatchAsync(model.ToCommand(userName, verificationToken), cancellationToken);
                return true;
            }
            catch (ServiceErrorException)
            {
                return false;
            }
        }
    }
}
