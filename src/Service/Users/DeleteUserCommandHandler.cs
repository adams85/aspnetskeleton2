using System.Threading;
using System.Threading.Tasks;
using WebApp.Core.Helpers;

namespace WebApp.Service.Users
{
    internal sealed class DeleteUserCommandHandler : CommandHandler<DeleteUserCommand>
    {
        public override async Task HandleAsync(DeleteUserCommand command, CommandContext context, CancellationToken cancellationToken)
        {
            await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
            {
                var user = await dbContext.Users.GetByNameAsync(command.UserName, cancellationToken).ConfigureAwait(false);
                RequireExisting(user, c => c.UserName);

                dbContext.Users.Remove(user);

                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
