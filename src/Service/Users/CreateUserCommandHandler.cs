using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using WebApp.Core.Helpers;
using WebApp.Core.Infrastructure;
using WebApp.DataAccess.Entities;
using WebApp.Service.Helpers;
using WebApp.Service.Mailing;
using WebApp.Service.Mailing.Users;

namespace WebApp.Service.Users;

internal sealed class CreateUserCommandHandler : CommandHandler<CreateUserCommand>
{
    private readonly IMailSenderService _mailSenderService;
    private readonly IGuidProvider _guidProvider;
    private readonly IClock _clock;

    public CreateUserCommandHandler(IMailSenderService mailSenderService, IGuidProvider guidProvider, IClock clock)
    {
        _mailSenderService = mailSenderService ?? throw new ArgumentNullException(nameof(mailSenderService));
        _guidProvider = guidProvider ?? throw new ArgumentNullException(nameof(guidProvider));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public override async Task HandleAsync(CreateUserCommand command, CommandContext context, CancellationToken cancellationToken)
    {
        await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
        {
            var userExists = await dbContext.Users.FilterByName(command.UserName).AnyAsync(cancellationToken).ConfigureAwait(false);
            RequireUnique(userExists, c => c.UserName);

            var emailExists = await dbContext.Users.FilterByEmail(command.Email).AnyAsync(cancellationToken).ConfigureAwait(false);
            RequireUnique(emailExists, c => c.Email);

            var user = new User();

            user.UserName = command.UserName;
            user.Email = command.Email;
            user.Password = SecurityHelper.HashPassword(command.Password);
            user.IsApproved = command.IsApproved;
            if (!command.IsApproved)
                user.ConfirmationToken = SecurityHelper.GenerateToken(_guidProvider);

            var now = _clock.UtcNow;
            user.CreateDate = now;
            user.LastPasswordChangedDate = now;
            user.PasswordFailuresSinceLastSuccess = 0;
            user.IsLockedOut = false;

            dbContext.Users.Add(user);

            if (command.CreateProfile)
            {
                var profile = new Profile();

                profile.FirstName = command.FirstName;
                profile.LastName = command.LastName;

                user.Profile = profile;
            }

            if (!command.IsApproved)
            {
                await using ((await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)).AsAsyncDisposable(out var transaction).ConfigureAwait(false))
                using (var transactionCommittedCts = new CancellationTokenSource())
                {
                    await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    await _mailSenderService.EnqueueItemAsync(new UnapprovedUserCreatedMailModel
                    {
                        Culture = context.ExecutionContext.Culture.Name,
                        UICulture = context.ExecutionContext.UICulture.Name,
                        Name = user.Profile?.FirstName,
                        UserName = user.UserName,
                        Email = user.Email,
                        VerificationToken = user.ConfirmationToken!,
                    }, dbContext, new CancellationChangeToken(transactionCommittedCts.Token), cancellationToken).ConfigureAwait(false);

                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                    transactionCommittedCts.Cancel();
                }
            }
            else
            {
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            command.OnKeyGenerated?.Invoke(command, user.Id);
        }
    }
}
