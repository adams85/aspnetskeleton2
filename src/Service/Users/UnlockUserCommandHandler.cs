﻿using System.Threading;
using System.Threading.Tasks;
using WebApp.Core.Helpers;

namespace WebApp.Service.Users;

internal sealed class UnlockUserCommandHandler : CommandHandler<UnlockUserCommand>
{
    public override async Task HandleAsync(UnlockUserCommand command, CommandContext context, CancellationToken cancellationToken)
    {
        await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
        {
            var user = await dbContext.Users.GetByNameAsync(command.UserName, cancellationToken).ConfigureAwait(false);
            RequireExisting(user, c => c.UserName);

            if (!user.IsLockedOut)
                return;

            user.IsLockedOut = false;
            user.PasswordFailuresSinceLastSuccess = 0;

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
