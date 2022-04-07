using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.Helpers;
using WebApp.DataAccess.Entities;
using WebApp.Service.Users;

namespace WebApp.Service.Roles;

internal sealed class RemoveUsersFromRolesCommandHandler : CommandHandler<RemoveUsersFromRolesCommand>
{
    public override async Task HandleAsync(RemoveUsersFromRolesCommand command, CommandContext context, CancellationToken cancellationToken)
    {
        await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
        {
            var dbProperties = dbContext.GetDbProperties();

            var distinctUserNames = new HashSet<string>(dbProperties.CaseInsensitiveComparer);
            var userWhereBuilder = PredicateBuilder.New<User>(false);
            foreach (var userName in command.UserNames)
            {
                if (distinctUserNames.Add(userName))
                    userWhereBuilder.Or(UsersHelper.GetFilterByNameWhere(userName));
            }

            var distinctRoleNames = new HashSet<string>(dbProperties.CaseInsensitiveComparer);
            var roleWhereBuilder = PredicateBuilder.New<Role>(false);
            foreach (var roleName in command.RoleNames)
            {
                if (distinctRoleNames.Add(roleName))
                    roleWhereBuilder.Or(RolesHelper.GetFilterByNameWhere(roleName));
            }

            var userIds = await
            (
                from u in dbContext.Users.Where(userWhereBuilder)
                select u.Id
            ).ToArrayAsync(cancellationToken).ConfigureAwait(false);

            RequireValid(userIds.Length == distinctUserNames.Count, c => c.UserNames);

            var roleAndUserIdPairs = await
            (
                from r in dbContext.Roles.Where(roleWhereBuilder)
                from ur in r.Users!.DefaultIfEmpty()
                select ValueTuple.Create(r.Id, (int?)ur.UserId)
            ).ToArrayAsync<(int RoleId, int? UserId)>(cancellationToken).ConfigureAwait(false);

            var userIdsByRoleId = roleAndUserIdPairs.ToLookup(ur => ur.RoleId, ur => ur.UserId);

            RequireValid(userIdsByRoleId.Count == distinctRoleNames.Count, c => c.RoleNames);

            var userRolesToRemove = new List<UserRole>();

            foreach (var userIdGroup in userIdsByRoleId)
            {
                foreach (var userId in userIds)
                {
                    if (userIdGroup.Any(id => id == userId))
                        userRolesToRemove.Add(new UserRole { UserId = userId, RoleId = userIdGroup.Key });
                }
            }

            dbContext.UserRoles.RemoveRange(userRolesToRemove);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
