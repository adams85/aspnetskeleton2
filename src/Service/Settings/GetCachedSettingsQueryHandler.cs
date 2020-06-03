using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Service.Settings
{
    internal sealed class GetCachedSettingsQueryHandler : QueryHandler<GetCachedSettingsQuery, Dictionary<string, string?>>
    {
        public override async Task<Dictionary<string, string?>> HandleAsync(GetCachedSettingsQuery query, QueryContext context, CancellationToken cancellationToken)
        {
            var linq = context.DbContext.Settings;

            return await linq.ToDictionaryAsync(s => s.Name, s => s.Value, cancellationToken).ConfigureAwait(false);
        }
    }
}
