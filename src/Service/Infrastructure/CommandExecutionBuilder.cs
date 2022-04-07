using System;
using System.Collections.Generic;

namespace WebApp.Service.Infrastructure;

internal sealed class CommandExecutionBuilder
{
    private readonly List<(Predicate<Type>, CommandInterceptorFactory)> _interceptorFactories = new List<(Predicate<Type>, CommandInterceptorFactory)>();

    public CommandExecutionBuilder AddInterceptor(Predicate<Type> commandTypeFilter, CommandInterceptorFactory interceptorFactory)
    {
        _interceptorFactories.Add((commandTypeFilter, interceptorFactory));
        return this;
    }

    public CommandExecutionBuilder AddInterceptorFor<TCommandBase>(CommandInterceptorFactory interceptorFactory) where TCommandBase : ICommand =>
        AddInterceptor(typeof(TCommandBase).IsAssignableFrom, interceptorFactory);

    public IReadOnlyList<(Predicate<Type>, CommandInterceptorFactory)> InterceptorFactories => _interceptorFactories;
}
