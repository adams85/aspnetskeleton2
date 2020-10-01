using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using WebApp.Common.Settings;
using WebApp.DataAccess.Entities;

namespace WebApp.Service.Settings
{
    internal sealed class ListSettingsQueryHandler : ListQueryHandler<ListSettingsQuery, SettingData>
    {
        private readonly IStringLocalizerFactory _stringLocalizerFactory;

        public ListSettingsQueryHandler(IStringLocalizerFactory stringLocalizerFactory)
        {
            _stringLocalizerFactory = stringLocalizerFactory ?? throw new ArgumentNullException(nameof(stringLocalizerFactory));
        }

        public override async Task<ListResult<SettingData>> HandleAsync(ListSettingsQuery query, QueryContext context, CancellationToken cancellationToken)
        {
            var settingEnumStringLocalizer = _stringLocalizerFactory.Create(typeof(SettingEnumConstants));

            // we translate descriptions at this point and include them in the query because we want to enable sorting/filtering on DB-side
            var nameToDescriptionMapper = SettingsHelper.BuildNameToDescriptionMapper(settingEnumStringLocalizer);

            IQueryable<Setting> linq = context.DbContext.Settings;

            return await ResultAsync(query, linq.ToData(nameToDescriptionMapper), cancellationToken).ConfigureAwait(false);
        }
    }
}
