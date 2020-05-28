using System;
using System.Threading;
using System.Threading.Tasks;
using WebApp.Core.Infrastructure;
using WebApp.Service.Helpers;

namespace WebApp.Service.Users
{
    internal sealed class ChangePasswordCommandHandler : CommandHandler<ChangePasswordCommand>
    {
        private readonly IClock _clock;

        public ChangePasswordCommandHandler(IClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public async Task HandleAsync(ChangePasswordCommand command, CommandContext context, CancellationToken cancellationToken)
        {
            var user = await context.DbContext.Users.GetByNameAsync(command.UserName, cancellationToken).ConfigureAwait(false);
            RequireExisting(user, c => c.UserName);

            var now = _clock.UtcNow;
            if (command.Verify)
            {
                RequireValid(
                    now < user.PasswordVerificationTokenExpirationDate && string.Equals(user.PasswordVerificationToken, command.VerificationToken, StringComparison.Ordinal),
                    c => c.VerificationToken);

                user.PasswordVerificationToken = null;
                user.PasswordVerificationTokenExpirationDate = null;

                user.PasswordFailuresSinceLastSuccess = 0;
                user.LastPasswordFailureDate = null;
                user.IsLockedOut = false;
            }

            user.Password = SecurityHelper.HashPassword(command.NewPassword);
            user.LastPasswordChangedDate = now;

            await context.DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
