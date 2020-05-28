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
    internal delegate Task CommandExecutionDelegate(CommandContext context, CancellationToken cancellationToken);

    internal delegate object CommandInterceptorFactory(IServiceProvider serviceProvider, CommandExecutionDelegate next);

    internal sealed class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly IReadOnlyList<(Predicate<Type> CommandTypeFilter, CommandInterceptorFactory InterceptorFactory)> _interceptorFactories;
        private readonly ConcurrentDictionary<Type, InterceptorChain> _interceptorChains;
        private readonly Func<Type, InterceptorChain> _cachedInterceptorChainFactory;

        public CommandDispatcher(IServiceProvider serviceProvider, IOptions<CommandDispatcherOptions>? options)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(_serviceProvider));

            _interceptorFactories = (options?.Value.InterceptorFactories ?? Enumerable.Empty<(Predicate<Type>, CommandInterceptorFactory)>()).ToArray();
            _interceptorChains = new ConcurrentDictionary<Type, InterceptorChain>();
            _cachedInterceptorChainFactory = type => new InterceptorChain(this, type);
        }

        public async Task DispatchAsync(ICommand command, CancellationToken cancellationToken)
        {
            using (var context = new CommandContext(command, _serviceProvider))
            {
                var interceptorChain = _interceptorChains.GetOrAdd(context.CommandType, _cachedInterceptorChainFactory);

                await interceptorChain.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            }
        }

        private sealed class InterceptorChain
        {
            private static readonly PropertyInfo s_contextScopedServicesProperty = Lambda.Property((CommandContext context) => context.ScopedServices);
            private static readonly PropertyInfo s_contextCommandProperty = Lambda.Property((CommandContext context) => context.Command);

            private static CommandExecutionDelegate BuildExecutionDelegate(object target, bool isHandler)
            {
                var type = target.GetType();
                var methodName = isHandler ? "HandleAsync" : "InvokeAsync";
                MethodInfo? method;

                try { method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public); }
                catch (AmbiguousMatchException) { method = null; }

                if (method == null)
                    throw new InvalidOperationException($"Type {type} declares no or multiple {methodName} methods.");

                var methodReturnType = typeof(Task);
                if (method.ReturnType != methodReturnType)
                    throw new InvalidOperationException($"Method {type}.{methodName} must return {methodReturnType}.");

                return method.BuildMethodInjectionDelegate<CommandExecutionDelegate>(
                    _ => Expression.Constant(target),
                    (delegateParams, methodParam, _) =>
                    {
                        if (methodParam.ParameterType == typeof(CommandContext))
                            return delegateParams[0];

                        if (methodParam.ParameterType == typeof(CancellationToken))
                            return delegateParams[1];

                        if (typeof(ICommand).IsAssignableFrom(methodParam.ParameterType))
                            return Expression.Convert(Expression.MakeMemberAccess(delegateParams[0], s_contextCommandProperty), methodParam.ParameterType);

                        return null;
                    },
                    delegateParams => Expression.MakeMemberAccess(delegateParams[0], s_contextScopedServicesProperty));
            }

            public InterceptorChain(CommandDispatcher dispatcher, Type commandType)
            {
                var handlerType = typeof(CommandHandler<>).MakeGenericType(commandType);

                var handler = dispatcher._serviceProvider.GetRequiredService(handlerType);
                var executionDelegate = BuildExecutionDelegate(handler, isHandler: true);

                for (var i = dispatcher._interceptorFactories.Count - 1; i >= 0; i--)
                {
                    var (commandTypeFilter, interceptorFactory) = dispatcher._interceptorFactories[i];
                    if (commandTypeFilter(commandType))
                    {
                        var interceptor = interceptorFactory(dispatcher._serviceProvider, executionDelegate);
                        executionDelegate = BuildExecutionDelegate(interceptor, isHandler: false);
                    }
                }

                ExecuteAsync = executionDelegate;
            }

            public CommandExecutionDelegate ExecuteAsync { get; }
        }
    }
}
