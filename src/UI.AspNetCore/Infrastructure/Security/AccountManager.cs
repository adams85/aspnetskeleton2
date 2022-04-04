using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WebApp.Service;
using WebApp.Service.Infrastructure;
using WebApp.Service.Infrastructure.Validation;
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

        public async Task<CachedUserInfoData?> GetCachedUserInfoAsync(string userName, bool registerActivity, CancellationToken cancellationToken)
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

        public async Task<AuthenticateUserStatus> ValidateUserAsync(NetworkCredential credentials, CancellationToken cancellationToken)
        {
            var authResult = await _queryDispatcher.DispatchAsync(new AuthenticateUserQuery
            {
                UserName = credentials.UserName,
                Password = credentials.Password,
            }, cancellationToken);

            if (authResult.UserId != null)
            {
                var authSuccess = authResult.Status == AuthenticateUserStatus.Successful;

                if (authSuccess || authResult.Status == AuthenticateUserStatus.Failed)
                    await _commandDispatcher.DispatchAsync(new RegisterUserActivityCommand
                    {
                        UserName = credentials.UserName,
                        SuccessfulLogin = authSuccess,
                        UIActivity = authSuccess,
                    }, CancellationToken.None);
            }

            return authResult.Status;
        }

        public async Task<(CreateUserStatus, PasswordRequirementsData?)> CreateUserAsync(RegisterModel model, CancellationToken cancellationToken)
        {
            try
            {
                await _commandDispatcher.DispatchAsync(model.ToCommand(), cancellationToken);
                return (CreateUserStatus.Success, null);
            }
            catch (ServiceErrorException ex)
            {
                switch (ex.ErrorCode)
                {
                    case ServiceErrorCode.ParamNotSpecified:
                    case ServiceErrorCode.ParamNotValid:
                        switch ((string)ex.Args[0])
                        {
                            case nameof(CreateUserCommand.UserName):
                                return (CreateUserStatus.InvalidUserName, null);
                            case nameof(CreateUserCommand.Email):
                                return (CreateUserStatus.InvalidEmail, null);
                            case nameof(CreateUserCommand.Password):
                                return (CreateUserStatus.InvalidPassword, ex.Args.Length > 1 ? ex.Args[^1] as PasswordRequirementsData : null);
                        }
                        break;
                    case ServiceErrorCode.EntityNotUnique:
                        switch ((string)ex.Args[0])
                        {
                            case nameof(CreateUserCommand.UserName):
                                return (CreateUserStatus.DuplicateUserName, null);
                            case nameof(CreateUserCommand.Email):
                                return (CreateUserStatus.DuplicateEmail, null);
                        }
                        break;
                }

                return (CreateUserStatus.UnexpectedError, null);
            }
        }

        public async Task<(ChangePasswordStatus, PasswordRequirementsData?)> ChangePasswordAsync(string userName, ChangePasswordModel model, CancellationToken cancellationToken)
        {
            var authStatus = await ValidateUserAsync(new NetworkCredential(userName, model.CurrentPassword), cancellationToken);

            switch (authStatus)
            {
                case AuthenticateUserStatus.NotExists:
                    return (ChangePasswordStatus.UserNotExists, null);
                case AuthenticateUserStatus.Unapproved:
                    return (ChangePasswordStatus.UserUnapproved, null);
                case AuthenticateUserStatus.LockedOut:
                    return (ChangePasswordStatus.UserLockedOut, null);
                case AuthenticateUserStatus.Failed:
                    return (ChangePasswordStatus.InvalidCredentials, null);
                case AuthenticateUserStatus.Successful:
                    break;
                default:
                    return (ChangePasswordStatus.UnexpectedError, null);
            }

            try
            {
                await _commandDispatcher.DispatchAsync(model.ToCommand(userName), cancellationToken);
                return (ChangePasswordStatus.Success, null);
            }
            catch (ServiceErrorException ex)
            {
                switch (ex.ErrorCode)
                {
                    case ServiceErrorCode.ParamNotSpecified:
                    case ServiceErrorCode.ParamNotValid:
                        switch ((string)ex.Args[0])
                        {
                            case nameof(ChangePasswordCommand.NewPassword):
                                return (ChangePasswordStatus.InvalidNewPassword, ex.Args.Length > 1 ? ex.Args[^1] as PasswordRequirementsData : null);
                        }
                        break;
                }

                return (ChangePasswordStatus.UnexpectedError, null);
            }
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
                switch (ex.ErrorCode)
                {
                    case ServiceErrorCode.EntityNotFound:
                        switch ((string)ex.Args[0])
                        {
                            // when user doesn't exist, success should be display to the user to prevent probing for existing accounts
                            case nameof(ResetPasswordCommand.UserName):
                                return true;
                        }
                        break;
                }

                return false;
            }
        }

        public async Task<(ChangePasswordStatus, PasswordRequirementsData?)> SetPasswordAsync(string userName, string verificationToken, SetPasswordModel model, CancellationToken cancellationToken)
        {
            try
            {
                await _commandDispatcher.DispatchAsync(model.ToCommand(userName, verificationToken), cancellationToken);
                return (ChangePasswordStatus.Success, null);
            }
            catch (ServiceErrorException ex)
            {
                switch (ex.ErrorCode)
                {
                    case ServiceErrorCode.ParamNotSpecified:
                    case ServiceErrorCode.ParamNotValid:
                        switch ((string)ex.Args[0])
                        {
                            case nameof(ChangePasswordCommand.NewPassword):
                                return (ChangePasswordStatus.InvalidNewPassword, ex.Args.Length > 1 ? ex.Args[^1] as PasswordRequirementsData : null);
                            case nameof(ChangePasswordCommand.VerificationToken):
                                return (ChangePasswordStatus.InvalidCredentials, null);
                        }
                        break;
                }

                return (ChangePasswordStatus.UnexpectedError, null);
            }
        }
    }
}
