using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WebApp.Core.Helpers;
using WebApp.Service.Helpers;

namespace WebApp.Service.Users;

internal sealed class AuthenticateUserQueryHandler : QueryHandler<AuthenticateUserQuery, AuthenticateUserResult>
{
    public override async Task<AuthenticateUserResult> HandleAsync(AuthenticateUserQuery query, QueryContext context, CancellationToken cancellationToken)
    {
        await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
        {
            var user = await dbContext.Users.GetByNameAsync(query.UserName, cancellationToken).ConfigureAwait(false);

            var result = new AuthenticateUserResult();

            if (user == null)
                return new AuthenticateUserResult { Status = AuthenticateUserStatus.NotExists };

            AuthenticateUserStatus status;
            if (!user.IsApproved)
                status = AuthenticateUserStatus.Unapproved;
            else if (user.IsLockedOut)
                status = AuthenticateUserStatus.LockedOut;
            else if (user.Password != null && SecurityHelper.VerifyHashedPassword(user.Password, query.Password) == PasswordVerificationResult.Success)
                status = AuthenticateUserStatus.Successful;
            else
                status = AuthenticateUserStatus.Failed;

            return new AuthenticateUserResult { UserId = user.Id, Status = status };
        }
    }
}
