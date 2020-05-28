using System;
using System.Threading;
using System.Threading.Tasks;
using WebApp.DataAccess.Entities;

namespace WebApp.Service.Users
{
    internal sealed class GetUserQueryHandler : QueryHandler<GetUserQuery, UserData?>
    {
        public async Task<UserData?> HandleAsync(GetUserQuery query, QueryContext context, CancellationToken cancellationToken)
        {
            User user;
            switch (query.Identifier)
            {
                case UserIdentifier.Key keyIdentifier:
                    user = await context.DbContext.FindAsync<User>(new object[] { keyIdentifier.Value }, cancellationToken).ConfigureAwait(false);
                    break;
                case UserIdentifier.Name nameIdentifier:
                    user = await context.DbContext.Users.GetByNameAsync(nameIdentifier.Value, cancellationToken).ConfigureAwait(false);
                    break;
                case UserIdentifier.Email emailIdentifier:
                    user = await context.DbContext.Users.GetByEmailAsync(emailIdentifier.Value, cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    RequireValid(false, q => q.Identifier);
                    throw new InvalidOperationException();
            }

            return user?.ToData();
        }
    }
}
