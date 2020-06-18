using System;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using WebApp.Service.Infrastructure;
using WebApp.Service.Infrastructure.Caching;
using WebApp.Service.Roles;

namespace WebApp.Service.Users
{
    internal sealed class GetCachedUserInfoQueryInvalidatorInterceptor : CachedQueryInvalidatorInterceptor
    {
        public GetCachedUserInfoQueryInvalidatorInterceptor(CommandExecutionDelegate next, ICache cache, Type[]? queryTypes)
            : base(next, cache, queryTypes) { }

        private string[]? GetAffectedUserNames(CommandContext context) => context.Command switch
        {
            CreateUserCommand createUserCommand => new[] { createUserCommand.UserName },
            ApproveUserCommand approveUserCommand => new[] { approveUserCommand.UserName },
            LockUserCommand lockUserCommand => new[] { lockUserCommand.UserName },
            UnlockUserCommand unlockUserCommand => new[] { unlockUserCommand.UserName },
            ChangePasswordCommand changePasswordCommand => changePasswordCommand.Verify ? new[] { changePasswordCommand.UserName } : null,
            RegisterUserActivityCommand registerUserActivityCommand => registerUserActivityCommand.SuccessfulLogin == false ? new[] { registerUserActivityCommand.UserName } : null,
            DeleteUserCommand deleteUserCommand => new[] { deleteUserCommand.UserName },
            AddUsersToRolesCommand addUsersToRolesCommand => addUsersToRolesCommand.UserNames,
            RemoveUsersFromRolesCommand removeUsersFromRolesCommand => removeUsersFromRolesCommand.UserNames,
            _ => throw new NotImplementedException()
        };

        protected override Task InvalidateQueryCacheAsync(CommandContext context, Type queryType, CancellationToken cancellationToken)
        {
            var userNames = GetAffectedUserNames(context);

            if (ArrayUtils.IsNullOrEmpty(userNames))
                return Task.CompletedTask;

            var tasks = Array.ConvertAll(userNames, un => Cache.RemoveScopeAsync(QueryCacherInterceptor.GetCacheScope(queryType, un), cancellationToken));
            return Task.WhenAll(tasks);
        }
    }
}
