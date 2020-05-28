using System;
using System.Threading;
using System.Threading.Tasks;
using WebApp.DataAccess.Entities;
using WebApp.Service.Users;

namespace WebApp.Service.Roles
{
    internal sealed class GetRoleQueryHandler : QueryHandler<GetRoleQuery, RoleData?>
    {
        public async Task<RoleData?> HandleAsync(GetRoleQuery query, QueryContext context, CancellationToken cancellationToken)
        {
            Role role;
            switch (query.Identifier)
            {
                case RoleIdentifier.Key keyIdentifier:
                    role = await context.DbContext.FindAsync<Role>(new[] { keyIdentifier.Value }, cancellationToken).ConfigureAwait(false);
                    break;
                case RoleIdentifier.Name nameIdentifier:
                    role = await context.DbContext.Roles.GetByNameAsync(nameIdentifier.Value, cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    RequireValid(false, q => q.Identifier);
                    throw new InvalidOperationException();
            }

            return role?.ToData();
        }
    }
}
