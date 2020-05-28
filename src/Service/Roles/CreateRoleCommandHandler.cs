using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Entities;

namespace WebApp.Service.Roles
{
    internal sealed class CreateRoleCommandHandler : CommandHandler<CreateRoleCommand>
    {
        public async Task HandleAsync(CreateRoleCommand command, CommandContext context, CancellationToken cancellationToken)
        {
            var roleExists = await context.DbContext.Roles.FilterByName(command.RoleName).AnyAsync(cancellationToken).ConfigureAwait(false);
            RequireUnique(roleExists, c => c.RoleName);

            var role = new Role
            {
                RoleName = command.RoleName,
                Description = command.Description,
            };

            context.DbContext.Roles.Add(role);

            await context.DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            command.OnKeyGenerated?.Invoke(command, role.Id);
        }
    }
}
