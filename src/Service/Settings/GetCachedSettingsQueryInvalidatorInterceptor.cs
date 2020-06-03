using System;
using System.Threading;
using System.Threading.Tasks;
using WebApp.Service.Infrastructure;
using WebApp.Service.Infrastructure.Caching;
using WebApp.Service.Infrastructure.Events;

namespace WebApp.Service.Settings
{
    internal sealed class GetCachedSettingsQueryInvalidatorInterceptor : CachedQueryInvalidatorInterceptor
    {
        private readonly IEventNotifier _eventNotifier;

        public GetCachedSettingsQueryInvalidatorInterceptor(CommandExecutionDelegate next, ICache cache, IEventNotifier eventNotifier, Type[]? queryTypes)
            : base(next, cache, queryTypes)
        {
            _eventNotifier = eventNotifier ?? throw new ArgumentNullException(nameof(eventNotifier));
        }

        protected override async Task InvalidateQueryCacheAsync(CommandContext context, Type queryType, CancellationToken cancellationToken)
        {
             await base.InvalidateQueryCacheAsync(context, queryType, cancellationToken).ConfigureAwait(false);

            _eventNotifier.Notify(new SettingsChangedEvent { });
        }
    }
}
