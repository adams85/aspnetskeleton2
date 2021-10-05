using System;
using System.Threading;
using System.Threading.Tasks;
using WebApp.Core.Helpers;
using WebApp.DataAccess.Entities;

namespace WebApp.Service.Users
{
    internal sealed class GetUserQueryHandler : QueryHandler<GetUserQuery, UserData?>
    {
        public override async Task<UserData?> HandleAsync(GetUserQuery query, QueryContext context, CancellationToken cancellationToken)
        {
            await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
            {
                User user;
                switch (query.Identifier)
                {
                    case UserIdentifier.Key keyIdentifier:
                        user = await dbContext.FindAsync<User>(new object[] { keyIdentifier.Value }, cancellationToken).ConfigureAwait(false);
                        break;
                    case UserIdentifier.Name nameIdentifier:
                        user = await dbContext.Users.GetByNameAsync(nameIdentifier.Value, cancellationToken).ConfigureAwait(false);
                        break;
                    case UserIdentifier.Email emailIdentifier:
                        user = await dbContext.Users.GetByEmailAsync(emailIdentifier.Value, cancellationToken).ConfigureAwait(false);
                        break;
                    default:
                        RequireValid(false, q => q.Identifier);
                        throw new InvalidOperationException();
                }

                return user?.ToData();
            }
        }
    }
}
