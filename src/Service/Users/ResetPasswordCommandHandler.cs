using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using WebApp.Core.Helpers;
using WebApp.Core.Infrastructure;
using WebApp.Service.Helpers;
using WebApp.Service.Mailing;
using WebApp.Service.Mailing.Users;

namespace WebApp.Service.Users
{
    internal sealed class ResetPasswordCommandHandler : CommandHandler<ResetPasswordCommand>
    {
        private readonly IMailSenderService _mailSenderService;
        private readonly IGuidProvider _guidProvider;
        private readonly IClock _clock;

        public ResetPasswordCommandHandler(IMailSenderService mailSenderService, IGuidProvider guidProvider, IClock clock)
        {
            _mailSenderService = mailSenderService ?? throw new ArgumentNullException(nameof(mailSenderService));
            _guidProvider = guidProvider ?? throw new ArgumentNullException(nameof(guidProvider));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public override async Task HandleAsync(ResetPasswordCommand command, CommandContext context, CancellationToken cancellationToken)
        {
            await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
            {
                var userWithProfile = await
                (
                    from u in dbContext.Users.FilterByName(command.UserName)
                    select new { User = u, u.Profile }
                ).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                RequireExisting(userWithProfile, c => c.UserName);

                var user = userWithProfile.User;

                user.PasswordVerificationToken = SecurityHelper.GenerateToken(_guidProvider);
                user.PasswordVerificationTokenExpirationDate = _clock.UtcNow + command.TokenExpirationTimeSpan;

                await using ((await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)).AsAsyncDisposable(out var transaction).ConfigureAwait(false))
                using (var transactionCommittedCts = new CancellationTokenSource())
                {
                    await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    await _mailSenderService.EnqueueItemAsync(new PasswordResetMailModel
                    {
                        Culture = context.ExecutionContext.Culture.Name,
                        UICulture = context.ExecutionContext.UICulture.Name,
                        Name = userWithProfile.Profile?.FirstName,
                        UserName = user.UserName,
                        Email = user.Email,
                        VerificationToken = user.PasswordVerificationToken,
                        VerificationTokenExpirationDate = user.PasswordVerificationTokenExpirationDate.Value,
                    }, dbContext, new CancellationChangeToken(transactionCommittedCts.Token), cancellationToken).ConfigureAwait(false);

                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                    transactionCommittedCts.Cancel();
                }
            }
        }
    }
}
