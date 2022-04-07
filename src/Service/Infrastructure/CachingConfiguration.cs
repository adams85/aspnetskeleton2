using WebApp.Service.Infrastructure.Caching;
using WebApp.Service.Roles;
using WebApp.Service.Users;

namespace WebApp.Service.Infrastructure;

internal static class CachingConfiguration
{
    public static CachingBuilder ConfigureQueryCaching(this CachingBuilder builder, CacheOptions defaultCacheOptions)
    {
        builder.Cache<GetCachedUserInfoQuery>()
            .InvalidatedBy<CreateUserCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
            .InvalidatedBy<ApproveUserCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
            .InvalidatedBy<LockUserCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
            .InvalidatedBy<UnlockUserCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
            .InvalidatedBy<ChangePasswordCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
            .InvalidatedBy<RegisterUserActivityCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
            .InvalidatedBy<DeleteUserCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
            .InvalidatedBy<AddUsersToRolesCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
            .InvalidatedBy<RemoveUsersFromRolesCommand, GetCachedUserInfoQueryInvalidatorInterceptor>()
            .InvalidatedBy<DeleteRoleCommand>()
            .WithScope(query => query.UserName)
            .WithSlidingExpiration(defaultCacheOptions.SlidingExpiration);

        return builder;
    }
}
