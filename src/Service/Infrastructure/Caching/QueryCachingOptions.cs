using System;
using System.Collections.Generic;

namespace WebApp.Service.Infrastructure.Caching;

internal abstract class QueryCachingOptions : CacheOptions
{
    protected QueryCachingOptions() { }

    public abstract bool TryHandleError(QueryContext context, Exception exception, out object? result);
    public abstract bool IsCached(QueryContext context);
    public abstract IEnumerable<string> GetScopes(QueryContext context);
}
