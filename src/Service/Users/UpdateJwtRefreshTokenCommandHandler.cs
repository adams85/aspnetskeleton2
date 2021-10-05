using System;
using System.Threading;
using System.Threading.Tasks;
using WebApp.Core.Helpers;
using WebApp.Core.Infrastructure;
using WebApp.Service.Helpers;

namespace WebApp.Service.Users
{
    internal sealed class UpdateJwtRefreshTokenCommandHandler : CommandHandler<UpdateJwtRefreshTokenCommand>
    {
        private readonly IGuidProvider _guidProvider;
        private readonly IClock _clock;

        public UpdateJwtRefreshTokenCommandHandler(IGuidProvider guidProvider, IClock clock)
        {
            _guidProvider = guidProvider ?? throw new ArgumentNullException(nameof(guidProvider));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public override async Task HandleAsync(UpdateJwtRefreshTokenCommand command, CommandContext context, CancellationToken cancellationToken)
        {
            await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
            {
                var user = await dbContext.Users.GetByNameAsync(command.UserName, cancellationToken).ConfigureAwait(false);
                RequireExisting(user, c => c.UserName);

                var now = _clock.UtcNow;
                if (command.Verify)
                {
                    RequireValid(
                        now < user.JwtRefreshTokenExpirationDate && string.Equals(user.JwtRefreshToken, command.VerificationToken, StringComparison.Ordinal),
                        c => c.VerificationToken);
                }

                user.JwtRefreshToken = SecurityHelper.GenerateToken(_guidProvider);
                user.JwtRefreshTokenExpirationDate = now + command.TokenExpirationTimeSpan;

                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                command.OnKeyGenerated?.Invoke(command, user.JwtRefreshToken);
            }
        }
    }
}
