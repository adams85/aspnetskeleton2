using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Karambolo.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using WebApp.Core.Helpers;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess
{
    internal abstract class EFCoreConfiguration
    {
        private static readonly IReadOnlyDictionary<string, IConfigurationFactory> s_configurationFactories = typeof(EFCoreConfiguration).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.HasInterface(typeof(IConfigurationFactory)))
            .Select(type => (IConfigurationFactory)Activator.CreateInstance(type))
            .ToDictionary(factory => factory.ProviderName, Identity<IConfigurationFactory>.Func);

        public static EFCoreConfiguration From(DataAccessOptions options)
        {
            if (!s_configurationFactories.TryGetValue(options.Database.Provider, out var configurationFactory))
                throw new NotSupportedException($"Database provider {options.Database.Provider} is not supported.");

            return configurationFactory.Create(options);
        }

        public DataAccessOptions Options { get; }

        protected EFCoreConfiguration(DataAccessOptions options)
        {
            Options = options;
        }

        protected abstract void ConfigureInternalServices(IServiceCollection internalServices, IServiceProvider applicationServiceProvider);

        protected IServiceCollection ReplaceDefaultRelationalTransactionFactory(IServiceCollection internalServices)
        {
            internalServices.ReplaceLast(ServiceDescriptor.Singleton<IRelationalTransactionFactory, ExtendedRelationalTransactionFactory>(), out var replacedDescriptor);

            Debug.Assert(replacedDescriptor != null &&
                (replacedDescriptor.ImplementationType ??
                 replacedDescriptor.ImplementationInstance?.GetType() ??
                 replacedDescriptor.ImplementationFactory?.GetType().GenericTypeArguments[1]) == typeof(RelationalTransactionFactory),
                 $"{Options.Database.Provider} doesn't use the default {nameof(IRelationalTransactionFactory)}.");

            return internalServices;
        }

        protected virtual IServiceProvider CreateInternalServiceProvider(IServiceProvider applicationServiceProvider)
        {
            var internalServices = new ServiceCollection();
            ConfigureInternalServices(internalServices, applicationServiceProvider);
            return internalServices.BuildServiceProvider();
        }

        public EFCoreConfiguration ConfigureServices<TContext>(IServiceCollection services) where TContext : DbContext
        {
            services.TryAddSingleton<InternalServiceProviderRegistry>();

            if (Options.DbContextLifetime == null)
                services.AddDbContextPool<TContext>(ConfigureOptions);
            else
                services.AddDbContext<TContext>(ConfigureOptions, Options.DbContextLifetime.Value);

            return this;
        }

        protected abstract void ConfigureOptionsCore(DbContextOptionsBuilder optionsBuilder, IServiceProvider internalServiceProvider, IServiceProvider applicationServiceProvider);

        private void ConfigureOptions(IServiceProvider applicationServiceProvider, DbContextOptionsBuilder optionsBuilder)
        {
            var internalServiceProvider = applicationServiceProvider.GetRequiredService<InternalServiceProviderRegistry>().GetOrCreateServiceProvider(this);

            optionsBuilder.UseInternalServiceProvider(internalServiceProvider);

            if (Options.EnableSqlLogging)
                optionsBuilder.EnableSensitiveDataLogging();
            else
                // https://stackoverflow.com/questions/47893481/change-logging-level-of-sql-queries-logged-by-entity-framework-core
                optionsBuilder.ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Trace)));

            ConfigureOptionsCore(optionsBuilder, internalServiceProvider, applicationServiceProvider);
        }

        private sealed class InternalServiceProviderRegistry : IDisposable, IAsyncDisposable
        {
            private readonly IServiceProvider _applicationServiceProvider;
            private readonly ConcurrentDictionary<EFCoreConfiguration, IServiceProvider> _internalServiceProviders;

            public InternalServiceProviderRegistry(IServiceProvider applicationServiceProvider)
            {
                _applicationServiceProvider = applicationServiceProvider;
                _internalServiceProviders = new ConcurrentDictionary<EFCoreConfiguration, IServiceProvider>();
            }

            public void Dispose()
            {
                foreach (var serviceProvider in _internalServiceProviders.Values)
                    if (serviceProvider is IDisposable disposable)
                        disposable.Dispose();
            }

            public async ValueTask DisposeAsync()
            {
                foreach (var serviceProvider in _internalServiceProviders.Values)
                    if (serviceProvider is IDisposable disposable)
                        await DisposableAdapter.From(disposable).DisposeAsync().ConfigureAwait(false);
            }

            public IServiceProvider GetOrCreateServiceProvider(EFCoreConfiguration configuration) =>
                _internalServiceProviders.GetOrAdd(configuration, key => key.CreateInternalServiceProvider(_applicationServiceProvider));
        }

        protected interface IConfigurationFactory
        {
            string ProviderName { get; }
            EFCoreConfiguration Create(DataAccessOptions options);
        }
    }
}
