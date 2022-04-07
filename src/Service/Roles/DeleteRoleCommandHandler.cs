using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.Helpers;

namespace WebApp.Service.Roles;

internal sealed class DeleteRoleCommandHandler : CommandHandler<DeleteRoleCommand>
{
    public override async Task HandleAsync(DeleteRoleCommand command, CommandContext context, CancellationToken cancellationToken)
    {
        await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
        {
            var role = await dbContext.Roles.GetByNameAsync(command.RoleName, cancellationToken).ConfigureAwait(false);
            RequireExisting(role, c => c.RoleName);

            if (!command.DeletePopulatedRole)
            {
                var hasUsers = await dbContext.UserRoles.AnyAsync(ur => ur.RoleId == role.Id, cancellationToken).ConfigureAwait(false);
                RequireIndependent(hasUsers, c => c.RoleName);
            }

            dbContext.Roles.Remove(role);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
