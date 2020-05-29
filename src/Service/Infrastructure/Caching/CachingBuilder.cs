using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp.Service.Infrastructure.Caching
{
    internal sealed class CachingBuilder
    {
        private readonly Dictionary<Type, QueryCachingBuilder> _builders = new Dictionary<Type, QueryCachingBuilder>();

        public QueryCachingBuilder<TQuery> Cache<TQuery>()
            where TQuery : IQuery
        {
            return Cache<TQuery, QueryCacherInterceptor>();
        }

        public QueryCachingBuilder<TQuery> Cache<TQuery, TInterceptor>()
            where TQuery : IQuery
            where TInterceptor : QueryCacherInterceptor
        {
            var config = new QueryCachingBuilder<TQuery>(typeof(TInterceptor));
            _builders.Add(typeof(TQuery), config);
            return config;
        }

        public void Build(InterceptorConfiguration interceptorConfiguration)
        {
            var invalidatorDescriptors = new Dictionary<KeyValuePair<Type, Type>, List<Type>>();

            foreach (var (queryType, builder) in _builders)
            {
                var options = builder.BuildOptions();

                var queryInterceptorActivator = ActivatorUtilities.CreateFactory(builder.QueryInterceptorType, new[] { typeof(QueryExecutionDelegate), typeof(QueryCachingOptions) });
                QueryInterceptorFactory queryInterceptorFactory = (sp, next) => (IQueryInterceptor)queryInterceptorActivator(sp, new object[] { next, options });
                interceptorConfiguration.QueryInterceptorFactories.Add((queryType.IsAssignableFrom, queryInterceptorFactory));

                foreach (var key in builder.Invalidators)
                {
                    if (!invalidatorDescriptors.TryGetValue(key, out var invalidatorDescriptor))
                        invalidatorDescriptors.Add(key, invalidatorDescriptor = new List<Type>());

                    invalidatorDescriptor.Add(queryType);
                }
            }

            foreach (var ((commandType, commandInterceptorType), queryTypeList) in invalidatorDescriptors)
            {
                var queryTypes = queryTypeList.ToArray();

                var commandInterceptorActivator = ActivatorUtilities.CreateFactory(commandInterceptorType, new[] { typeof(CommandExecutionDelegate), typeof(Type[]) });
                CommandInterceptorFactory commandInterceptorFactory = (sp, next) => (ICommandInterceptor)commandInterceptorActivator(sp, new object[] { next, queryTypes });
                interceptorConfiguration.CommandInterceptorFactories.Add((commandType.IsAssignableFrom, commandInterceptorFactory));
            }
        }
    }
}
