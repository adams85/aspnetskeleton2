using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebApp.Core.Helpers;

namespace WebApp.Service.Infrastructure
{
    internal delegate Task<object?> QueryExecutionDelegate(QueryContext context, CancellationToken cancellationToken);

    internal delegate object QueryInterceptorFactory(IServiceProvider serviceProvider, QueryExecutionDelegate next);

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
            using (var context = new QueryContext(query, _serviceProvider))
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
            private static readonly PropertyInfo s_contextQueryProperty = Lambda.Property((QueryContext context) => context.Query);

            private static readonly MethodInfo s_convertHandlerResultMethodDefinition =
                new Func<Task<object>, Task<object?>>(ConvertHandlerResult<object>).Method.GetGenericMethodDefinition();

            private static async Task<object?> ConvertHandlerResult<TResult>(Task<TResult> task) => await task.ConfigureAwait(false);

            private static QueryExecutionDelegate BuildExecutionDelegate(object target, Type resultType, bool isHandler)
            {
                var type = target.GetType();
                var methodName = isHandler ? "HandleAsync" : "InvokeAsync";
                MethodInfo? method;

                try { method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public); }
                catch (AmbiguousMatchException) { method = null; }

                if (method == null)
                    throw new InvalidOperationException($"Type {type} declares no or multiple {methodName} methods.");

                var methodReturnType = isHandler ? typeof(Task<>).MakeGenericType(resultType) : typeof(Task<object?>);
                if (method.ReturnType != methodReturnType)
                    throw new InvalidOperationException($"Method {type}.{methodName} must return {methodReturnType}.");

                var convertReturnType =
                    isHandler ?
                    (methodCall, _) => Expression.Call(s_convertHandlerResultMethodDefinition.MakeGenericMethod(resultType), methodCall) :
                    (Func<MethodCallExpression, Type, Expression>?)null;

                return method.BuildMethodInjectionDelegate<QueryExecutionDelegate>(
                    _ => Expression.Constant(target),
                    (delegateParams, methodParam, _) =>
                    {
                        if (methodParam.ParameterType == typeof(QueryContext))
                            return delegateParams[0];

                        if (methodParam.ParameterType == typeof(CancellationToken))
                            return delegateParams[1];

                        if (typeof(IQuery).IsAssignableFrom(methodParam.ParameterType))
                            return Expression.Convert(Expression.MakeMemberAccess(delegateParams[0], s_contextQueryProperty), methodParam.ParameterType);

                        return null;
                    },
                    delegateParams => Expression.MakeMemberAccess(delegateParams[0], s_contextScopedServicesProperty),
                    convertReturnType);
            }

            public InterceptorChain(QueryDispatcher dispatcher, Type queryType)
            {
                var resultType = QueryContext.GetResultType(queryType);
                var handlerType = typeof(QueryHandler<,>).MakeGenericType(queryType, resultType);

                var handler = dispatcher._serviceProvider.GetRequiredService(handlerType);
                var executionDelegate = BuildExecutionDelegate(handler, resultType, isHandler: true);

                for (var i = dispatcher._interceptorFactories.Count - 1; i >= 0; i--)
                {
                    var (queryTypeFilter, interceptorFactory) = dispatcher._interceptorFactories[i];
                    if (queryTypeFilter(queryType))
                    {
                        var interceptor = interceptorFactory(dispatcher._serviceProvider, executionDelegate);
                        executionDelegate = BuildExecutionDelegate(interceptor, resultType, isHandler: false);
                    }
                }

                ExecuteAsync = executionDelegate;
            }

            public QueryExecutionDelegate ExecuteAsync { get; }
        }
    }
}
