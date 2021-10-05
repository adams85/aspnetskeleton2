using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.Helpers;
using WebApp.DataAccess.Entities;

namespace WebApp.Service.Roles
{
    internal sealed class CreateRoleCommandHandler : CommandHandler<CreateRoleCommand>
    {
        public override async Task HandleAsync(CreateRoleCommand command, CommandContext context, CancellationToken cancellationToken)
        {
            await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
            {
                var roleExists = await dbContext.Roles.FilterByName(command.RoleName).AnyAsync(cancellationToken).ConfigureAwait(false);
                RequireUnique(roleExists, c => c.RoleName);

                var role = new Role
                {
                    RoleName = command.RoleName,
                    Description = command.Description,
                };

                dbContext.Roles.Add(role);

                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                command.OnKeyGenerated?.Invoke(command, role.Id);
            }
        }
    }
}
