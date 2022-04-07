using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using Microsoft.Extensions.Options;
using WebApp.Core.Helpers;

namespace WebApp.Service.Infrastructure;

internal sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    private readonly IReadOnlyList<(Predicate<Type> QueryTypeFilter, QueryInterceptorFactory InterceptorFactory)> _interceptorFactories;
    private readonly ConcurrentDictionary<Type, InterceptorChain> _interceptorChains;
    private readonly Func<Type, InterceptorChain> _cachedInterceptorChainFactory;

    public QueryDispatcher(IServiceProvider serviceProvider, IOptions<QueryDispatcherOptions>? options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(_serviceProvider));

        _interceptorFactories = (options?.Value.InterceptorFactories ?? Enumerable.Empty<(Predicate<Type>, QueryInterceptorFactory)>()).ToArray();
        _interceptorChains = new ConcurrentDictionary<Type, InterceptorChain>();
        _cachedInterceptorChainFactory = type => new InterceptorChain(this, type);
    }

    public async Task<object?> DispatchAsync(IQuery query, CancellationToken cancellationToken)
    {
        await using (new QueryContext(query, _serviceProvider).AsAsyncDisposable(out var context).ConfigureAwait(false))
        {
            var interceptorChain = _interceptorChains.GetOrAdd(context.QueryType, _cachedInterceptorChainFactory);

            return await interceptorChain.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken) =>
        (TResult)(await DispatchAsync((IQuery)query, cancellationToken).ConfigureAwait(false))!;

    private sealed class InterceptorChain
    {
        private static readonly PropertyInfo s_contextScopedServicesProperty = Lambda.Property((QueryContext context) => context.ScopedServices);

        private static QueryExecutionDelegate BuildExecutionDelegate(IQueryInterceptor target)
        {
            var type = target.GetType();
            const string methodName = "InvokeAsync";
            MethodInfo? method;

            try { method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public); }
            catch (AmbiguousMatchException) { method = null; }

            if (method == null)
                throw new InvalidOperationException($"Type {type} declares no or multiple {methodName} methods.");

            var methodReturnType = typeof(Task<object?>);
            if (method.ReturnType != methodReturnType)
                throw new InvalidOperationException($"Method {type}.{methodName} must return {methodReturnType}.");

            return method.BuildMethodInjectionDelegate<QueryExecutionDelegate>(
                getTargetInstance: _ => Expression.Constant(target),
                getStaticArguments: (delegateParams, methodParam, _) =>
                {
                    if (methodParam.ParameterType == typeof(QueryContext))
                        return delegateParams[0];

                    if (methodParam.ParameterType == typeof(CancellationToken))
                        return delegateParams[1];

                    return null;
                },
                getServiceProvider: delegateParams => Expression.MakeMemberAccess(delegateParams[0], s_contextScopedServicesProperty));
        }

        public InterceptorChain(QueryDispatcher dispatcher, Type queryType)
        {
            var resultType = QueryContext.GetResultType(queryType);

            var handlerInvokerInterceptorType = typeof(HandlerInvokerInterceptor<,>).MakeGenericType(queryType, resultType);
            var interceptor = (IQueryInterceptor)Activator.CreateInstance(handlerInvokerInterceptorType)!;
            var executionDelegate = BuildExecutionDelegate(interceptor);

            for (var i = dispatcher._interceptorFactories.Count - 1; i >= 0; i--)
            {
                var (queryTypeFilter, interceptorFactory) = dispatcher._interceptorFactories[i];
                if (queryTypeFilter(queryType))
                {
                    interceptor = interceptorFactory(dispatcher._serviceProvider, executionDelegate);
                    executionDelegate = BuildExecutionDelegate(interceptor);
                }
            }

            ExecuteAsync = executionDelegate;
        }

        public QueryExecutionDelegate ExecuteAsync { get; }
    }

    private sealed class HandlerInvokerInterceptor<TQuery, TResult> : IQueryInterceptor
        where TQuery : IQuery<TResult>
    {
        public async Task<object?> InvokeAsync(QueryHandler<TQuery, TResult> handler, QueryContext context, CancellationToken cancellationToken) =>
            await handler.HandleAsync((TQuery)context.Query, context, cancellationToken).ConfigureAwait(false);
    }
}
