using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApp.DataAccess.Entities;

namespace WebApp.Service.Settings
{
    internal sealed class ListSettingsQueryHandler : ListQueryHandler<ListSettingsQuery, SettingData>
    {
        public override async Task<ListResult<SettingData>> HandleAsync(ListSettingsQuery query, QueryContext context, CancellationToken cancellationToken)
        {
            IQueryable<Setting> linq = context.DbContext.Settings;

            return await ResultAsync(query, linq.ToData(), cancellationToken).ConfigureAwait(false);
        }
    }
}
