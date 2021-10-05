using System;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace WebApp.Service.Infrastructure.Caching
{
    internal class CachedQueryInvalidatorInterceptor : ICommandInterceptor
    {
        private readonly CommandExecutionDelegate _next;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly Type[] _queryTypes;
        private readonly ILogger _logger;

        public CachedQueryInvalidatorInterceptor(CommandExecutionDelegate next, IHostApplicationLifetime applicationLifetime, ICache cache, Type[]? queryTypes,
            ILogger<CachedQueryInvalidatorInterceptor>? logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
            Cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _queryTypes = queryTypes ?? Type.EmptyTypes;
            _logger = logger ?? (ILogger)NullLogger.Instance;
        }

        protected ICache Cache { get; }

        protected virtual Task InvalidateQueryCacheAsync(CommandContext context, Type queryType, CancellationToken cancellationToken)
        {
            return Cache.RemoveScopeAsync(QueryCacherInterceptor.GetCacheScope(queryType), cancellationToken);
        }

        private void InvalidateQueryCache(CommandContext context)
        {
            var tasks = Array.ConvertAll(_queryTypes, qt => InvalidateQueryCacheAsync(context, qt, _applicationLifetime.ApplicationStopping));

            Task.WhenAll(tasks).FireAndForget(ex => _logger.LogError(ex, "{COMMAND_TYPE} was unable to invalidate cache of {QUERY_TYPES}.",
                context.CommandType,
                string.Join(", ", (object[])_queryTypes)));
        }

        public async Task InvokeAsync(CommandContext context, CancellationToken cancellationToken)
        {
            await _next(context, cancellationToken).ConfigureAwait(false);

            InvalidateQueryCache(context);
        }
    }
}
