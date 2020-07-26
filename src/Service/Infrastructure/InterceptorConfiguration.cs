using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApp.Core.Infrastructure;
using WebApp.Service.Infrastructure.Caching;
using WebApp.Service.Infrastructure.Logging;
using WebApp.Service.Infrastructure.Validation;

namespace WebApp.Service.Infrastructure
{
    internal static class InterceptorConfiguration
    {
        public static CommandExecutionBuilder ConfigureInterceptors(this CommandExecutionBuilder builder, Action<CommandExecutionBuilder>? addCachedQueryInvalidatorInterceptors)
        {
            // order matters: the earlier an interceptor is registered, the earlier it runs during execution

            builder.AddInterceptorFor<ICommand>((sp, next) => new CommandPerformanceLoggerInterceptor(next, sp.GetRequiredService<IGuidProvider>(), sp.GetRequiredService<ILogger<CommandPerformanceLoggerInterceptor>>()));

            builder.AddInterceptorFor<ICommand>((sp, next) => new CommandDataAnnotationsValidatorInterceptor(next));

            // query cache invalidator interceptors should be as close to the handler as possible!
            addCachedQueryInvalidatorInterceptors?.Invoke(builder);

            return builder;
        }

        public static QueryExecutionBuilder ConfigureInterceptors(this QueryExecutionBuilder builder, Action<QueryExecutionBuilder>? addQueryCacherInterceptors)
        {
            // order matters: the earlier an interceptor is registered, the earlier it runs during execution

            // query cacher interceptors should be as close to the caller as possible!
            addQueryCacherInterceptors?.Invoke(builder);

            builder.AddInterceptorFor<IQuery>((sp, next) => new QueryPerformanceLoggerInterceptor(next, sp.GetRequiredService<IGuidProvider>(), sp.GetRequiredService<ILogger<QueryPerformanceLoggerInterceptor>>()));

            builder.AddInterceptorFor<IQuery>((sp, next) => new QueryDataAnnotationsValidatorInterceptor(next));

            return builder;
        }

        public sealed class ConfigureDispatcherOptions : IConfigureOptions<CommandDispatcherOptions>, IConfigureOptions<QueryDispatcherOptions>
        {
            private readonly IReadOnlyList<(Predicate<Type>, CommandInterceptorFactory)> _commandInterceptorFactories;
            private readonly IReadOnlyList<(Predicate<Type>, QueryInterceptorFactory)> _queryInterceptorFactories;

            public ConfigureDispatcherOptions(IOptions<CacheOptions>? defaultCacheOptions)
            {
                var (addCachedQueryInvalidatorInterceptors, addQueryCacherInterceptors) = new CachingBuilder()
                    .ConfigureQueryCaching(defaultCacheOptions?.Value ?? CacheOptions.Default)
                    .Build();

                _commandInterceptorFactories = new CommandExecutionBuilder()
                    .ConfigureInterceptors(addCachedQueryInvalidatorInterceptors)
                    .InterceptorFactories;

                _queryInterceptorFactories = new QueryExecutionBuilder()
                    .ConfigureInterceptors(addQueryCacherInterceptors)
                    .InterceptorFactories;
            }

            public void Configure(CommandDispatcherOptions options) => options.InterceptorFactories.AddRange(_commandInterceptorFactories);

            public void Configure(QueryDispatcherOptions options) => options.InterceptorFactories.AddRange(_queryInterceptorFactories);
        }
    }
}
