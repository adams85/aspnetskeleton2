using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Users
{
    internal sealed class UnlockUserCommandHandler : CommandHandler<UnlockUserCommand>
    {
        public override async Task HandleAsync(UnlockUserCommand command, CommandContext context, CancellationToken cancellationToken)
        {
            var user = await context.DbContext.Users.GetByNameAsync(command.UserName, cancellationToken).ConfigureAwait(false);
            RequireExisting(user, c => c.UserName);

            if (!user.IsLockedOut)
                return;

            user.IsLockedOut = false;
            user.PasswordFailuresSinceLastSuccess = 0;

            await context.DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
