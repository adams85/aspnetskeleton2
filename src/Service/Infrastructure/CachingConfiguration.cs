using System;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using WebApp.Service.Infrastructure.Caching;
using WebApp.Service.Roles;
using WebApp.Service.Users;

namespace WebApp.Service.Infrastructure
{
    internal static class CachingConfiguration
    {
        public static void ConfigureQueryCaching(this InterceptorConfiguration interceptorConfiguration, CacheOptions defaultCacheOptions)
        {
            var builder = new CachingBuilder();

            builder.Cache<GetCachedUserInfoQuery>()
                .WithScope(q => q.UserName)
                .InvalidatedBy<ApproveUserCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<LockUserCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<UnlockUserCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<ChangePasswordCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<RegisterUserActivityCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<DeleteUserCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<AddUsersToRolesCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<RemoveUsersFromRolesCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
                .InvalidatedBy<DeleteRoleCommand>()
                .WithSlidingExpiration(defaultCacheOptions.SlidingExpiration);

            builder.Build(interceptorConfiguration);
        }

        public sealed class GetCachedUserInfoQueryInvalidatorInterceptor : CachedQueryInvalidatorInterceptor
        {
            public GetCachedUserInfoQueryInvalidatorInterceptor(CommandExecutionDelegate next, ICache cache, Type[] queryTypes)
                : base(next, cache, queryTypes) { }

            private string[]? GetAffectedUserNames(CommandContext context) => context.Command switch
            {
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

                if (!ArrayUtils.IsNullOrEmpty(userNames))
                {
                    var tasks = Array.ConvertAll(userNames, un => Cache.RemoveScopeAsync(QueryCacherInterceptor.GetCacheScope(queryType, un), cancellationToken));
                    return Task.WhenAll(tasks);
                }
                else
                    return Task.CompletedTask;
            }
        }
    }
}
