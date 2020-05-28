using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Service.Users
{
    internal sealed class GetOnlineUserCountQueryHandler : QueryHandler<GetOnlineUserCountQuery, int>
    {
        public async Task<int> HandleAsync(GetOnlineUserCountQuery query, QueryContext context, CancellationToken cancellationToken)
        {
            return await context.DbContext.Users.CountAsync(u => u.LastActivityDate > query.DateFrom, cancellationToken).ConfigureAwait(false);
        }
    }
}
