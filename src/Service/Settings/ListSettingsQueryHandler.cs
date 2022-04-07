using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using WebApp.Common.Settings;
using WebApp.Core.Helpers;
using WebApp.DataAccess.Entities;

namespace WebApp.Service.Settings;

internal sealed class ListSettingsQueryHandler : ListQueryHandler<ListSettingsQuery, SettingData>
{
    private readonly IStringLocalizerFactory _stringLocalizerFactory;

    public ListSettingsQueryHandler(IStringLocalizerFactory stringLocalizerFactory)
    {
        _stringLocalizerFactory = stringLocalizerFactory ?? throw new ArgumentNullException(nameof(stringLocalizerFactory));
    }

    public override async Task<ListResult<SettingData>> HandleAsync(ListSettingsQuery query, QueryContext context, CancellationToken cancellationToken)
    {
        await using (context.CreateDbContext().AsAsyncDisposable(out var dbContext).ConfigureAwait(false))
        {
            IQueryable<Setting> linq = dbContext.Settings;

            if (query.NamePattern != null)
                linq = linq.Where(setting => setting.Name!.ToLower().Contains(query.NamePattern.ToLower()));

            if (query.ValuePattern != null)
                linq = linq.Where(setting => setting.Value!.ToLower().Contains(query.ValuePattern.ToLower()));

            var settingEnumStringLocalizer = _stringLocalizerFactory.Create(typeof(SettingEnumConstants));

            // we translate descriptions at this point and include them in the query because we want to enable sorting/filtering on DB-side
            var nameToDescriptionMapper = SettingsHelper.BuildNameToDescriptionMapperExpression(settingEnumStringLocalizer);

            var resultLinq = linq.ToData(nameToDescriptionMapper);

            if (query.DescriptionPattern != null)
                resultLinq = resultLinq.Where(setting => setting.Description!.ToLower().Contains(query.DescriptionPattern.ToLower()));

            return await ResultAsync(query, resultLinq, cancellationToken).ConfigureAwait(false);
        }
    }
}
