using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using WebApp.DataAccess;
using WebApp.DataAccess.Providers.Sqlite;
using WebApp.Service.Infrastructure.Database;
using WebApp.Service.Tests.Infrastructure.Database;
using WebApp.Tests.Helpers;

namespace WebApp.Service.Tests.Infrastructure
{
    /// <summary>
    /// Builder for test case context configuration.
    /// </summary>
    public class TestContextBuilder
    {
        public static readonly DataAccessOptions DefaultDataAccessOptions = new DataAccessOptions
        {
            Database = new DbOptions
            {
                Provider = SqliteProperties.ProviderName,
                ConnectionString = new SqliteConnectionStringBuilder { DataSource = SqliteProperties.InMemoryDataSource }.ToString(),
            },
            DbContextLifetime = ServiceLifetime.Transient,
            EnableSqlLogging = true
        };

        public static readonly OptionsProvider DefaultOptionsProvider = new OptionsProvider(DefaultDataAccessOptions);

        public static TestContextBuilder CreateDefault(Action<TestContextBuilder>? configure = null)
        {
            var builder = new TestContextBuilder()
                .ConfigureDefaultLogging();

            configure?.Invoke(builder);

            return builder;
        }

        private Dictionary<string, string?>? _configurationValues;
        private Action<IConfigurationBuilder>? _onConfiguring;
        private Action<IConfiguration, ServiceCollection>? _onConfigured;
        private Action<ILoggingBuilder>? _configureLogging;

        public ServiceCollection Services { get; } = new ServiceCollection();

        public TestContextBuilder AddServices(Action<ServiceCollection> configure)
        {
            configure(Services);
            return this;
        }

        public TestContextBuilder UseConfiguration(string key, string? value)
        {
            (_configurationValues ??= new Dictionary<string, string?>())[key] = value;
            return this;
        }

        public TestContextBuilder OnConfiguring(Action<IConfigurationBuilder> configure)
        {
            _onConfiguring += configure;
            return this;
        }

        public TestContextBuilder OnConfigured(Action<IConfiguration, ServiceCollection> configure)
        {
            _onConfigured += configure;
            return this;
        }

        public TestContextBuilder ConfigureLogging(Action<ILoggingBuilder> configure)
        {
            _configureLogging += configure;
            return this;
        }

        public TestContextBuilder ConfigureDefaultLogging() =>
            ConfigureLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.ClearProviders();
                builder.AddDebug();
            });

        public TestContextBuilder AddInitializer<TInitializer>()
            where TInitializer : class, ITestInitializer
        {
            Services.TryAddEnumerable(ServiceDescriptor.Singleton<ITestInitializer, TInitializer>());
            return this;
        }

        public TestDatabaseBuilder AddDatabase(bool addDataAccessServices = true)
        {
            if (addDataAccessServices)
                Services.AddDataAccess(DefaultOptionsProvider);

            Services.Configure<DbInitializerOptions>(options => options.Seed = DbSeedObjects.DbObjects | DbSeedObjects.BaseData);

            return new TestDatabaseBuilder(this)
                .AddSeeder(sp => new DelegatedDatabaseSeeder((ctx, ct) => ctx.Database.EnsureCreatedAsync(ct)));
        }

        /// <summary>
        /// Builds the test case context and executes the initialization logic registered by (<see cref="AddInitializer{TInitializer}"/>.
        /// </summary>
        public async Task<TestContext> BuildAsync(CancellationToken cancellationToken = default)
        {
            if (_onConfiguring != null || _configurationValues != null)
            {
                var configurationBuilder = new ConfigurationBuilder();

                _onConfiguring?.Invoke(configurationBuilder);

                if (_configurationValues != null)
                    configurationBuilder.AddInMemoryCollection(_configurationValues);

                var configuration = configurationBuilder.Build();

                Services.AddSingleton<IConfiguration>(configuration);
                Services.AddOptions();

                _onConfigured?.Invoke(configuration, Services);
            }

            if (_configureLogging != null)
                Services.AddLogging(_configureLogging);

            var services = Services.BuildServiceProvider();

            var context = new TestContext(services);

            var initializers = context.Services.GetRequiredService<IEnumerable<ITestInitializer>>();
            foreach (var initializer in initializers)
                await initializer.InitializeAsync(context, cancellationToken);

            return context;
        }
    }
}
