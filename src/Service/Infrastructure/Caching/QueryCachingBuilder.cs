using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace WebApp.Service.Infrastructure.Caching
{
    internal delegate bool QueryErrorHandler(QueryContext context, Exception exception, out object? result);

    internal abstract class QueryCachingBuilder
    {
        protected QueryCachingBuilder(Type interceptorType)
        {
            QueryInterceptorType = interceptorType;
        }

        public Type QueryInterceptorType { get; }
        public Dictionary<Type, Type> Invalidators { get; } = new Dictionary<Type, Type>();

        public abstract QueryCachingOptions BuildOptions();
    }

    internal sealed class QueryCachingBuilder<TQuery> : QueryCachingBuilder
        where TQuery : IQuery
    {
        private TimeSpan? _absoluteExpiration;
        private TimeSpan? _slidingExpiration;
        private CacheItemPriority? _priority;

        private Func<TQuery, bool>? _filter;
        private readonly List<Func<TQuery, string>> _scopeSelectors = new List<Func<TQuery, string>>();
        private QueryErrorHandler? _errorHandler;

        public QueryCachingBuilder(Type interceptorType) : base(interceptorType) { }

        public QueryCachingBuilder<TQuery> OnError(QueryErrorHandler handler)
        {
            _errorHandler = handler;
            return this;
        }

        public QueryCachingBuilder<TQuery> When(Func<TQuery, bool> filter)
        {
            _filter = filter;
            return this;
        }

        public QueryCachingBuilder<TQuery> WithScope(Func<TQuery, string> selector)
        {
            _scopeSelectors.Add(selector);
            return this;
        }

        public QueryCachingBuilder<TQuery> WithAbsoluteExpiration(TimeSpan? value)
        {
            _absoluteExpiration = value;
            return this;
        }

        public QueryCachingBuilder<TQuery> WithSlidingExpiration(TimeSpan? value)
        {
            _slidingExpiration = value;
            return this;
        }

        public QueryCachingBuilder<TQuery> WithPriority(CacheItemPriority? value)
        {
            _priority = value;
            return this;
        }

        public QueryCachingBuilder<TQuery> InvalidatedBy<TCommand>()
            where TCommand : ICommand
        {
            return InvalidatedBy<TCommand, CachedQueryInvalidatorInterceptor>();
        }

        public QueryCachingBuilder<TQuery> InvalidatedBy<TCommand, TInterceptor>()
            where TCommand : ICommand
            where TInterceptor : CachedQueryInvalidatorInterceptor
        {
            Invalidators.Add(typeof(TCommand), typeof(TInterceptor));
            return this;
        }

        public override QueryCachingOptions BuildOptions()
        {
            return new Options
            {
                AbsoluteExpiration = _absoluteExpiration,
                SlidingExpiration = _slidingExpiration,
                Priority = _priority,
                ErrorHandler = _errorHandler,
                ScopeSelectors = _scopeSelectors.ToArray(),
                Filter = _filter
            };
        }

        private sealed class Options : QueryCachingOptions
        {
            public QueryErrorHandler? ErrorHandler { get; set; }
            public Func<TQuery, string>[] ScopeSelectors { get; set; } = null!;
            public Func<TQuery, bool>? Filter { get; set; }

            public override bool IsCached(QueryContext context)
            {
                return Filter?.Invoke((TQuery)context.Query) ?? true;
            }

            public override IEnumerable<string> GetScopes(QueryContext context)
            {
                yield return QueryCacherInterceptor.GetCacheScope(context.QueryType);

                var query = (TQuery)context.Query;
                for (int i = 0, n = ScopeSelectors.Length; i < n; i++)
                    yield return QueryCacherInterceptor.GetCacheScope(context.QueryType, ScopeSelectors[i](query));
            }

            public override bool TryHandleError(QueryContext context, Exception exception, out object? result)
            {
                if (ErrorHandler == null)
                {
                    result = default;
                    return false;
                }

                return ErrorHandler(context, exception, out result);
            }
        }
    }
}
