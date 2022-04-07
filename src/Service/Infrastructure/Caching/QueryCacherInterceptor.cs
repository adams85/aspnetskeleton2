using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApp.Core.Helpers;
using WebApp.Service.Infrastructure.Serialization;

namespace WebApp.Service.Infrastructure.Caching;

internal class QueryCacherInterceptor : IQueryInterceptor
{
    private readonly QueryExecutionDelegate _next;
    private readonly QueryCachingOptions _options;

    public static string GetCacheScope(Type queryType, params string[] subScopes)
    {
        return string.Join("|", subScopes.Prepend(queryType.FullNameWithoutAssemblyDetails()));
    }

    public QueryCacherInterceptor(QueryExecutionDelegate next, ICache cache, QueryCachingOptions options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    protected ICache Cache { get; }

    protected virtual string GetCacheKey(QueryContext context)
    {
        using var ms = new MemoryStream();
        InternalSerializer.CacheKey.Serialize(ms, context.Query, context.Query.GetType());
        var queryBytes = ms.GetBuffer().AsSpan(0, (int)ms.Length);
        return context.QueryType.FullNameWithoutAssemblyDetails() + "|" + Convert.ToBase64String(queryBytes);
    }

    protected virtual bool IsCached(QueryContext context)
    {
        return _options.IsCached(context);
    }

    protected virtual IEnumerable<string> GetScopes(QueryContext context)
    {
        return _options.GetScopes(context);
    }

    public Task<object?> InvokeAsync(QueryContext context, CancellationToken cancellationToken)
    {
        if (IsCached(context))
        {
            return Cache.GetOrAddAsync(GetCacheKey(context), async (k, ct) =>
            {
                try { return await _next(context, ct).ConfigureAwait(false); }
                catch (Exception ex) when (_options.TryHandleError(context, ex, out var result)) { return result; }
            }, _options, GetScopes(context), cancellationToken);
        }
        else
        {
            return _next(context, cancellationToken);
        }
    }
}
