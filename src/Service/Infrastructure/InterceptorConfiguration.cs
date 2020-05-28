using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApp.Service.Infrastructure.Caching;
using WebApp.Service.Infrastructure.Logging;
using WebApp.Service.Infrastructure.Validation;

namespace WebApp.Service.Infrastructure
{
    internal sealed class InterceptorConfiguration : IConfigureOptions<CommandDispatcherOptions>, IConfigureOptions<QueryDispatcherOptions>
    {
        private IEnumerable<(Predicate<Type>, CommandInterceptorFactory)> GetCommandInterceptorFactories()
        {
            yield return (typeof(ICommand).IsAssignableFrom, (sp, next) => new CommandPerformanceLoggerInterceptor(next, sp.GetRequiredService<ILoggerFactory>()));
            yield return (typeof(ICommand).IsAssignableFrom, (sp, next) => new CommandDataAnnotationsValidatorInterceptor(next));
        }

        private IEnumerable<(Predicate<Type>, QueryInterceptorFactory)> GetQueryInterceptorFactories()
        {
            yield return (typeof(IQuery).IsAssignableFrom, (sp, next) => new QueryPerformanceLoggerInterceptor(next, sp.GetRequiredService<ILoggerFactory>()));
            yield return (typeof(IQuery).IsAssignableFrom, (sp, next) => new QueryDataAnnotationsValidatorInterceptor(next));
        }

        public InterceptorConfiguration(IOptions<CacheOptions>? defaultCacheOptions)
        {
            // order matters: the earlier an interceptor is registered, the earlier it runs during execution

            // 1) other command interceptors
            CommandInterceptorFactories.AddRange(GetCommandInterceptorFactories());

            // 2) query caching interceptors (as close to the caller as possible)
            // 3) query cache invalidator interceptors (as close to the handler as possible)
            this.ConfigureQueryCaching(defaultCacheOptions?.Value ?? CacheOptions.Default);

            // 4) other query interceptors
            QueryInterceptorFactories.AddRange(GetQueryInterceptorFactories());
        }

        public List<(Predicate<Type>, CommandInterceptorFactory)> CommandInterceptorFactories { get; } = new List<(Predicate<Type>, CommandInterceptorFactory)>();
        public List<(Predicate<Type>, QueryInterceptorFactory)> QueryInterceptorFactories { get; } = new List<(Predicate<Type>, QueryInterceptorFactory)>();

        public void Configure(CommandDispatcherOptions options)
        {
            options.InterceptorFactories = CommandInterceptorFactories;
        }

        public void Configure(QueryDispatcherOptions options)
        {
            options.InterceptorFactories = QueryInterceptorFactories;
        }
    }
}
