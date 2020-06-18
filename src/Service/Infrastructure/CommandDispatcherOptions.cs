using System;
using System.Collections.Generic;

namespace WebApp.Service.Infrastructure
{
    internal class CommandDispatcherOptions
    {
        public List<(Predicate<Type> CommandTypeFilter, CommandInterceptorFactory InterceptorFactory)> InterceptorFactories { get; } =
            new List<(Predicate<Type>, CommandInterceptorFactory)>();
    }
}
