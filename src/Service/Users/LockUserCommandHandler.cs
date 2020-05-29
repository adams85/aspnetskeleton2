using System;
using System.Threading;
using System.Threading.Tasks;
using WebApp.Core.Infrastructure;

namespace WebApp.Service.Users
{
    internal sealed class LockUserCommandHandler : CommandHandler<LockUserCommand>
    {
        private readonly IClock _clock;

        public LockUserCommandHandler(IClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public override async Task HandleAsync(LockUserCommand command, CommandContext context, CancellationToken cancellationToken)
        {
            var user = await context.DbContext.Users.GetByNameAsync(command.UserName, cancellationToken).ConfigureAwait(false);
            RequireExisting(user, c => c.UserName);

            if (user.IsLockedOut)
                return;

            user.LastLockoutDate = _clock.UtcNow;
            user.IsLockedOut = true;

            await context.DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
