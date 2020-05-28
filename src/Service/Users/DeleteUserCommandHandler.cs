using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Users
{
    internal sealed class DeleteUserCommandHandler : CommandHandler<DeleteUserCommand>
    {
        public async Task HandleAsync(DeleteUserCommand command, CommandContext context, CancellationToken cancellationToken)
        {
            var user = await context.DbContext.Users.GetByNameAsync(command.UserName, cancellationToken).ConfigureAwait(false);
            RequireExisting(user, c => c.UserName);

            context.DbContext.Users.Remove(user);

            await context.DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
