using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using WebApp.Common.Settings;

namespace WebApp.Service.Settings
{
    internal sealed class GetSettingQueryHandler : QueryHandler<GetSettingQuery, SettingData?>
    {
        private readonly IStringLocalizerFactory _stringLocalizerFactory;

        public GetSettingQueryHandler(IStringLocalizerFactory stringLocalizerFactory)
        {
            _stringLocalizerFactory = stringLocalizerFactory ?? throw new ArgumentNullException(nameof(stringLocalizerFactory));
        }

        public override async Task<SettingData?> HandleAsync(GetSettingQuery query, QueryContext context, CancellationToken cancellationToken)
        {
            var setting = await context.DbContext.Settings.FirstOrDefaultAsync(setting => setting.Name == query.Name, cancellationToken).ConfigureAwait(false);

            Func<string, string?>? nameToDescriptionMapper;
            if (query.IncludeDescription)
            {
                var settingEnumStringLocalizer = _stringLocalizerFactory.Create(typeof(SettingEnumConstants));
                nameToDescriptionMapper = SettingsHelper.BuildNameToDescriptionMapper(settingEnumStringLocalizer);
            }
            else
                nameToDescriptionMapper = null;

            return setting?.ToData(nameToDescriptionMapper);
        }
    }
}
