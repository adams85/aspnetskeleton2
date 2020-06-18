using System;
using System.Collections.Generic;

namespace WebApp.Service.Infrastructure
{
    internal sealed class QueryExecutionBuilder
    {
        private readonly List<(Predicate<Type>, QueryInterceptorFactory)> _interceptorFactories = new List<(Predicate<Type>, QueryInterceptorFactory)>();

        public QueryExecutionBuilder AddInterceptor(Predicate<Type> queryTypeFilter, QueryInterceptorFactory interceptorFactory)
        {
            _interceptorFactories.Add((queryTypeFilter, interceptorFactory));
            return this;
        }

        public QueryExecutionBuilder AddInterceptorFor<TQueryBase>(QueryInterceptorFactory interceptorFactory) where TQueryBase : IQuery =>
            AddInterceptor(typeof(TQueryBase).IsAssignableFrom, interceptorFactory);

        public IReadOnlyList<(Predicate<Type>, QueryInterceptorFactory)> InterceptorFactories => _interceptorFactories;
    }
}
