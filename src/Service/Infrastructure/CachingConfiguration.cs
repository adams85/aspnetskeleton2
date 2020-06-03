using WebApp.Service.Contract.Settings;
using WebApp.Service.Infrastructure.Caching;
using WebApp.Service.Roles;
using WebApp.Service.Settings;
using WebApp.Service.Users;

namespace WebApp.Service.Infrastructure
{
    internal static class CachingConfiguration
    {
        public static void ConfigureQueryCaching(this InterceptorConfiguration interceptorConfiguration, CacheOptions defaultCacheOptions)
        {
            var builder = new CachingBuilder();

            builder.Cache<GetCachedSettingsQuery>()
                .InvalidatedBy<UpdateSettingCommand, GetCachedSettingsQueryInvalidatorInterceptor>()
                .WithSlidingExpiration(defaultCacheOptions.SlidingExpiration);

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
    }
}
