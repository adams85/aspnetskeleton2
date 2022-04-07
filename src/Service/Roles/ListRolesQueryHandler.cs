using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApp.Core.Helpers;
using WebApp.DataAccess.Entities;
using WebApp.Service.Roles;
using WebApp.Service.Users;

namespace WebApp.Service.Roles;

internal sealed class ListRolesQueryHandler : ListQueryHandler<ListRolesQuery, RoleData>
{
    public override async Task<ListResult<RoleData>> HandleAsync(ListRolesQuery query, QueryContext context, CancellationToken cancellationToken)
    {
        await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
        {
            IQueryable<Role> linq;
            if (query.UserName != null)
                linq =
                    from u in dbContext.Users.FilterByName(query.UserName)
                    from ur in u.Roles!
                    select ur.Role;
            else
                linq = dbContext.Roles;

            return await ResultAsync(query, linq.ToData(), cancellationToken).ConfigureAwait(false);
        }
    }
}
