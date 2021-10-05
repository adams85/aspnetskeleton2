using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.Helpers;

namespace WebApp.Service.Users
{
    internal sealed class GetOnlineUserCountQueryHandler : QueryHandler<GetOnlineUserCountQuery, int>
    {
        public override async Task<int> HandleAsync(GetOnlineUserCountQuery query, QueryContext context, CancellationToken cancellationToken)
        {
            await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
            {
                return await dbContext.Users.CountAsync(u => u.LastActivityDate > query.DateFrom, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
