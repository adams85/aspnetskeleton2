using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WebApp.DataAccess.Providers.Sqlite
{
    internal sealed class SqliteConfiguration : EFCoreConfiguration
    {
        private SqliteConfiguration(DataAccessOptions options) : base(options) { }

        protected override void ConfigureInternalServices(IServiceCollection internalServices, IServiceProvider applicationServiceProvider)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder(Options.Database.ConnectionString);
            if (":memory:".Equals(connectionStringBuilder.DataSource, StringComparison.OrdinalIgnoreCase))
                internalServices.AddSingleton(_ => new InMemoryConnectionWrapper(Options.Database));

            internalServices
                .AddEntityFrameworkSqlite()
                .ReplaceLast(ServiceDescriptor.Singleton<IModelCustomizer, SqliteModelCustomizer>())
                .AddSingleton<IDbProperties>(new SqliteProperties(Options.Database));
        }

        protected override void ConfigureOptionsCore(DbContextOptionsBuilder optionsBuilder, IServiceProvider internalServiceProvider, IServiceProvider applicationServiceProvider)
        {
            optionsBuilder.AddInterceptors(new SqliteConnectionInterceptor());

            var connectionWrapper = internalServiceProvider.GetService<InMemoryConnectionWrapper>();
            if (connectionWrapper == null)
                optionsBuilder.UseSqlite(Options.Database.ConnectionString);
            else
                optionsBuilder.UseSqlite(connectionWrapper.Connection);
        }

        private sealed class InMemoryConnectionWrapper : IDisposable
        {
            public InMemoryConnectionWrapper(DbOptions dbOptions)
            {
                Connection = new SqliteConnection(dbOptions.ConnectionString);
                Connection.Open();
                SqliteConnectionInterceptor.CreateCustomObjects(Connection);
            }

            public void Dispose()
            {
                Connection.Dispose();
            }

            public SqliteConnection Connection { get; }
        }

        public sealed class Factory : IConfigurationFactory
        {
            public string ProviderName => SqliteProperties.ProviderName;

            public EFCoreConfiguration Create(DataAccessOptions options) => new SqliteConfiguration(options);
        }
    }
}
