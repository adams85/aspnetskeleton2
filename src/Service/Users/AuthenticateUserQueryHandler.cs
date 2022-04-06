﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WebApp.Core.Helpers;
using WebApp.Service.Helpers;

namespace WebApp.Service.Users
{
    internal sealed class AuthenticateUserQueryHandler : QueryHandler<AuthenticateUserQuery, AuthenticateUserResult>
    {
        public override async Task<AuthenticateUserResult> HandleAsync(AuthenticateUserQuery query, QueryContext context, CancellationToken cancellationToken)
        {
            await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
            {
                var user = await dbContext.Users.GetByNameAsync(query.UserName, cancellationToken).ConfigureAwait(false);

                var result = new AuthenticateUserResult();

                if (user == null)
                {
                    result.Status = AuthenticateUserStatus.NotExists;
                    return result;
                }

                result.UserId = user.Id;
                if (!user.IsApproved)
                    result.Status = AuthenticateUserStatus.Unapproved;
                else if (user.IsLockedOut)
                    result.Status = AuthenticateUserStatus.LockedOut;
                else
                    result.Status =
                        user.Password != null && SecurityHelper.VerifyHashedPassword(user.Password, query.Password) == PasswordVerificationResult.Success ?
                        AuthenticateUserStatus.Successful :
                        AuthenticateUserStatus.Failed;

                return result;
            }
        }
    }
}
