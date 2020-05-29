using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Infrastructure.Caching
{
    internal class CachedQueryInvalidatorInterceptor : ICommandInterceptor
    {
        private readonly CommandExecutionDelegate _next;
        private readonly Type[] _queryTypes;

        public CachedQueryInvalidatorInterceptor(CommandExecutionDelegate next, ICache cache, Type[]? queryTypes)
        {
            Cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _next = next;
            _queryTypes = queryTypes ?? Type.EmptyTypes;
        }

        protected ICache Cache { get; }

        protected virtual Task InvalidateQueryCacheAsync(CommandContext context, Type queryType, CancellationToken cancellationToken)
        {
            return Cache.RemoveScopeAsync(QueryCacherInterceptor.GetCacheScope(queryType), cancellationToken);
        }

        public async Task InvokeAsync(CommandContext context, CancellationToken cancellationToken)
        {
            await _next(context, cancellationToken).ConfigureAwait(false);

            // cancellationToken: default -> if execution was successful, cache invalidation shouldn't be interrupted
            var tasks = Array.ConvertAll(_queryTypes, qt => InvalidateQueryCacheAsync(context, qt, default));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
