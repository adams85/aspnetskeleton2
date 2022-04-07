using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using WebApp.Core.Helpers;
using WebApp.Core.Infrastructure;
using WebApp.Service.Mailing;
using WebApp.Service.Mailing.Users;

namespace WebApp.Service.Users;

internal sealed class RegisterUserActivityCommandHandler : CommandHandler<RegisterUserActivityCommand>
{
    private readonly IMailSenderService _mailSenderService;
    private readonly IClock _clock;

    private readonly LockoutOptions? _lockoutOptions;

    public RegisterUserActivityCommandHandler(IMailSenderService mailSenderService, IClock clock, IOptions<LockoutOptions>? lockoutOptions)
    {
        _mailSenderService = mailSenderService ?? throw new ArgumentNullException(nameof(mailSenderService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));

        _lockoutOptions = lockoutOptions?.Value;
    }

    public override async Task HandleAsync(RegisterUserActivityCommand command, CommandContext context, CancellationToken cancellationToken)
    {
        await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
        {
            var user = await dbContext.Users.GetByNameAsync(command.UserName, cancellationToken).ConfigureAwait(false);
            RequireExisting(user, c => c.UserName);

            bool lockedOut = false;

            var now = _clock.UtcNow;
            if (command.SuccessfulLogin == true)
            {
                user.PasswordFailuresSinceLastSuccess = 0;
                user.LastLoginDate = now;
            }
            else if (command.SuccessfulLogin == false)
            {
                var failures = user.PasswordFailuresSinceLastSuccess;
                if (_lockoutOptions == null || failures < _lockoutOptions.MaxFailedAccessAttempts)
                {
                    user.PasswordFailuresSinceLastSuccess += 1;
                    user.LastPasswordFailureDate = now;
                }
                else
                {
                    user.LastPasswordFailureDate = now;
                    user.LastLockoutDate = now;
                    user.IsLockedOut = true;

                    lockedOut = true;
                }
            }

            if (command.UIActivity)
                user.LastActivityDate = now;

            if (lockedOut)
            {
                await using ((await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)).AsAsyncDisposable(out var transaction).ConfigureAwait(false))
                using (var transactionCommittedCts = new CancellationTokenSource())
                {
                    await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    await _mailSenderService.EnqueueItemAsync(new UserLockedOutMailModel
                    {
                        Culture = context.ExecutionContext.Culture.Name,
                        UICulture = context.ExecutionContext.UICulture.Name,
                        Name = user.Profile?.FirstName,
                        UserName = user.UserName,
                        Email = user.Email,
                    }, dbContext, new CancellationChangeToken(transactionCommittedCts.Token), cancellationToken).ConfigureAwait(false);

                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                    transactionCommittedCts.Cancel();
                }
            }
            else
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
