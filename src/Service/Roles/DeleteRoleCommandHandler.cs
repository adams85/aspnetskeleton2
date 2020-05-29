using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Service.Roles
{
    internal sealed class DeleteRoleCommandHandler : CommandHandler<DeleteRoleCommand>
    {
        public override async Task HandleAsync(DeleteRoleCommand command, CommandContext context, CancellationToken cancellationToken)
        {
            var role = await context.DbContext.Roles.GetByNameAsync(command.RoleName, cancellationToken).ConfigureAwait(false);
            RequireExisting(role, c => c.RoleName);

            if (!command.DeletePopulatedRole)
            {
                var hasUsers = await context.DbContext.UserRoles.AnyAsync(ur => ur.RoleId == role.Id, cancellationToken).ConfigureAwait(false);
                RequireIndependent(hasUsers, c => c.RoleName);
            }

            context.DbContext.Roles.Remove(role);

            await context.DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
