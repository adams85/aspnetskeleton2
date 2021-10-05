using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.Helpers;
using WebApp.DataAccess.Entities;
using WebApp.Service.Users;

namespace WebApp.Service.Roles
{
    internal sealed class IsUserInRoleQueryHandler : QueryHandler<IsUserInRoleQuery, bool>
    {
        public override async Task<bool> HandleAsync(IsUserInRoleQuery query, QueryContext context, CancellationToken cancellationToken)
        {
            await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
            {
                IQueryable<UserRole> linq = dbContext.UserRoles;

                Expression<Func<UserRole, User>> selectUser = ur => ur.User;
                linq = linq.Where(selectUser.Chain(UsersHelper.GetFilterByNameWhere(query.UserName)));

                Expression<Func<UserRole, Role>> selectRole = ur => ur.Role;
                linq = linq.Where(selectRole.Chain(RolesHelper.GetFilterByNameWhere(query.RoleName)));

                return await linq.AnyAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
