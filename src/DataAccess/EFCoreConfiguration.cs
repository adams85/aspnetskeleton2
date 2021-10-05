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
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using WebApp.Core.Helpers;

namespace WebApp.DataAccess
{
    internal abstract class EFCoreConfiguration
    {
        private static readonly IReadOnlyDictionary<string, IConfigurationFactory> s_configurationFactories = typeof(EFCoreConfiguration).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && type.HasInterface(typeof(IConfigurationFactory)))
            .Select(type => type.GetConstructor(Type.EmptyTypes))
            .Where(ctor => ctor != null)
            .Select(ctor => (IConfigurationFactory)ctor!.Invoke(null))
            .ToDictionary(factory => factory.ProviderName, CachedDelegates.Identity<IConfigurationFactory>.Func);

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

        protected virtual IServiceProvider CreateInternalServiceProvider(IServiceProvider applicationServiceProvider)
        {
            var internalServices = new ServiceCollection();
            ConfigureInternalServices(internalServices, applicationServiceProvider);
            return internalServices.BuildServiceProvider();
        }

        public EFCoreConfiguration ConfigureServices<TContext>(IServiceCollection services) where TContext : PooledDbContext
        {
            services.TryAddSingleton<InternalServiceProviderRegistry>();

            // we want to be explicit about DbContext lifetime (that is, we want to manage that instead of DI),
            // so for now we need to backport the IDbContextFactory concept introduced in .NET 5
            // TODO: replace the code below with AddPooledDbContextFactory after upgrading to .NET 5+

            services.TryAddSingleton(sp =>
            {
                var builder = new DbContextOptionsBuilder<TContext>(new DbContextOptions<TContext>(new Dictionary<Type, IDbContextOptionsExtension>()));

                builder.UseApplicationServiceProvider(sp);
                PooledDbContextFactory<TContext>.SetMaxPoolSize(builder, Options.DbContextPoolSize ?? PooledDbContextFactory<TContext>.DefaultPoolSize);

                ConfigureOptions(sp, builder);

                return builder.Options;
            });

            services.AddSingleton<DbContextOptions>(sp => sp.GetRequiredService<DbContextOptions<TContext>>());

            services.TryAddSingleton((IServiceProvider sp) => new DbContextPool<TContext>(sp.GetService<DbContextOptions<TContext>>()));

            services.TryAddSingleton<IDbContextFactory<TContext>>(
                sp => new PooledDbContextFactory<TContext>(sp.GetRequiredService<DbContextPool<TContext>>()));

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
                        await AsyncDisposableAdapter.From(disposable).DisposeAsync().ConfigureAwait(false);
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
