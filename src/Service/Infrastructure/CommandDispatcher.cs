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
        await using (new CommandContext(command, _serviceProvider).AsAsyncDisposable(out var context).ConfigureAwait(false))
        {
            var interceptorChain = _interceptorChains.GetOrAdd(context.CommandType, _cachedInterceptorChainFactory);

            await interceptorChain.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class InterceptorChain
    {
        private static readonly PropertyInfo s_contextScopedServicesProperty = Lambda.Property((CommandContext context) => context.ScopedServices);

        private static CommandExecutionDelegate BuildExecutionDelegate(ICommandInterceptor target)
        {
            var type = target.GetType();
            const string methodName = "InvokeAsync";
            MethodInfo? method;

            try { method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public); }
            catch (AmbiguousMatchException) { method = null; }

            if (method == null)
                throw new InvalidOperationException($"Type {type} declares no or multiple {methodName} methods.");

            var methodReturnType = typeof(Task);
            if (method.ReturnType != methodReturnType)
                throw new InvalidOperationException($"Method {type}.{methodName} must return {methodReturnType}.");

            return method.BuildMethodInjectionDelegate<CommandExecutionDelegate>(
                getTargetInstance: _ => Expression.Constant(target),
                getStaticArguments: (delegateParams, methodParam, _) =>
                {
                    if (methodParam.ParameterType == typeof(CommandContext))
                        return delegateParams[0];

                    if (methodParam.ParameterType == typeof(CancellationToken))
                        return delegateParams[1];

                    return null;
                },
                getServiceProvider: delegateParams => Expression.MakeMemberAccess(delegateParams[0], s_contextScopedServicesProperty));
        }

        public InterceptorChain(CommandDispatcher dispatcher, Type commandType)
        {
            var handlerInvokerInterceptorType = typeof(HandlerInvokerInterceptor<>).MakeGenericType(commandType);
            var interceptor = (ICommandInterceptor)Activator.CreateInstance(handlerInvokerInterceptorType)!;
            var executionDelegate = BuildExecutionDelegate(interceptor);

            for (var i = dispatcher._interceptorFactories.Count - 1; i >= 0; i--)
            {
                var (commandTypeFilter, interceptorFactory) = dispatcher._interceptorFactories[i];
                if (commandTypeFilter(commandType))
                {
                    interceptor = interceptorFactory(dispatcher._serviceProvider, executionDelegate);
                    executionDelegate = BuildExecutionDelegate(interceptor);
                }
            }

            ExecuteAsync = executionDelegate;
        }

        public CommandExecutionDelegate ExecuteAsync { get; }
    }

    private sealed class HandlerInvokerInterceptor<TCommand> : ICommandInterceptor
        where TCommand : ICommand
    {
        public Task InvokeAsync(CommandHandler<TCommand> handler, CommandContext context, CancellationToken cancellationToken) =>
            handler.HandleAsync((TCommand)context.Command, context, cancellationToken);
    }
}
