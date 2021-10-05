using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.Helpers;

namespace WebApp.Service.Users
{
    internal sealed class GetCachedUserInfoQueryHandler : QueryHandler<GetCachedUserInfoQuery, CachedUserInfoData?>
    {
        public override async Task<CachedUserInfoData?> HandleAsync(GetCachedUserInfoQuery query, QueryContext context, CancellationToken cancellationToken)
        {
            await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
            {
                var linq =
                    from u in dbContext.Users.FilterByName(query.UserName)
                    select new CachedUserInfoData
                    {
                        UserId = u.Id,
                        UserName = u.UserName,
                        Email = u.Email,
                        FirstName = u.Profile != null ? u.Profile.FirstName : null,
                        LastName = u.Profile != null ? u.Profile.LastName : null,
                        LoginAllowed = u.IsApproved && !u.IsLockedOut
                    };

                var result = await linq.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                if (result != null)
                {
                    var rolesLinq =
                        from u in dbContext.Users.Where(u => u.Id == result.UserId)
                        from ur in u.Roles
                        select ur.Role.RoleName;

                    result.Roles = await rolesLinq.ToArrayAsync(cancellationToken).ConfigureAwait(false);
                }

                return result;
            }
        }
    }
}
