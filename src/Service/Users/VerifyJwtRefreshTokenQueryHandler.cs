using System;
using System.Threading;
using System.Threading.Tasks;
using WebApp.Core.Helpers;
using WebApp.Core.Infrastructure;

namespace WebApp.Service.Users;

internal sealed class VerifyJwtRefreshTokenQueryHandler : QueryHandler<VerifyJwtRefreshTokenQuery, bool>
{
    private readonly IClock _clock;

    public VerifyJwtRefreshTokenQueryHandler(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public override async Task<bool> HandleAsync(VerifyJwtRefreshTokenQuery query, QueryContext context, CancellationToken cancellationToken)
    {
        await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
        {
            var user = await dbContext.Users.GetByNameAsync(query.UserName, cancellationToken).ConfigureAwait(false);
            RequireExisting(user, c => c.UserName);

            var now = _clock.UtcNow;
            return user.ValidateJwtRefreshToken(query.VerificationToken!, now);
        }
    }
}
