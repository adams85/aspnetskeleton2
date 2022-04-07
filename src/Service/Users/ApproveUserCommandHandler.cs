using System;
using System.Threading;
using System.Threading.Tasks;
using WebApp.Core.Helpers;

namespace WebApp.Service.Users;

internal sealed class ApproveUserCommandHandler : CommandHandler<ApproveUserCommand>
{
    public override async Task HandleAsync(ApproveUserCommand command, CommandContext context, CancellationToken cancellationToken)
    {
        await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
        {
            var user = await dbContext.Users.GetByNameAsync(command.UserName, cancellationToken).ConfigureAwait(false);
            RequireExisting(user, c => c.UserName);

            if (user.IsApproved)
                return;

            if (command.Verify)
                RequireValid(string.Equals(user.ConfirmationToken, command.VerificationToken, StringComparison.Ordinal), m => m.VerificationToken);

            user.ConfirmationToken = null;
            user.IsApproved = true;

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
