using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApp.Service.Roles;
using WebApp.Service.Users;

namespace WebApp.Service.Users
{
    internal sealed class ListUsersQueryHandler : ListQueryHandler<ListUsersQuery, UserData>
    {
        public async Task<ListResult<UserData>> HandleAsync(ListUsersQuery query, QueryContext context, CancellationToken cancellationToken)
        {
            var linq =
                query.RoleName != null ?
                (
                    from r in context.DbContext.Roles.FilterByName(query.RoleName)
                    from ur in r.Users
                    select ur.User
                ) :
                context.DbContext.Users;

            if (query.UserNamePattern != null)
                linq = linq.FilterByName(query.UserNamePattern, pattern: true);

            if (query.EmailPattern != null)
                linq = linq.FilterByEmail(query.EmailPattern, pattern: true);

            return await ResultAsync(query, linq.ToData(), cancellationToken).ConfigureAwait(false);
        }
    }
}
