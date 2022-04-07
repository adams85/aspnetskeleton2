using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.Sqlite;

internal sealed class SqliteConfiguration : EFCoreConfiguration
{
    private SqliteConfiguration(DataAccessOptions options) : base(options) { }

    protected override void ConfigureInternalServices(IServiceCollection internalServices, IServiceProvider applicationServiceProvider)
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder(Options.Database.ConnectionString);
        if (connectionStringBuilder.Mode == SqliteOpenMode.Memory && connectionStringBuilder.Cache == SqliteCacheMode.Shared)
            internalServices.AddSingleton(_ => new InMemoryConnectionWrapper(Options.Database));

        internalServices.AddEntityFrameworkSqlite();

        internalServices
            .ReplaceLast(ServiceDescriptor.Scoped<ISqliteRelationalConnection, CustomSqliteRelationalConnection>())
            .ReplaceLast(ServiceDescriptor.Singleton<IModelCustomizer, SqliteModelCustomizer>())
            .AddSingleton<IDbProperties>(new SqliteProperties(Options.Database));
    }

    protected override IServiceProvider CreateInternalServiceProvider(IServiceProvider applicationServiceProvider)
    {
        var internalServiceProvider = base.CreateInternalServiceProvider(applicationServiceProvider);

        // by resolving the connection wrapper (in case it's been registered), we ensure the connection which is responsible for keeping alive the shared in-memory DB instance:
        // "The database persists as long as at least one connection to it remains open."
        // https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/in-memory-databases#shareable-in-memory-databases
        internalServiceProvider.GetService<InMemoryConnectionWrapper>();

        return internalServiceProvider;
    }

    protected override void ConfigureOptionsCore(DbContextOptionsBuilder optionsBuilder, IServiceProvider internalServiceProvider, IServiceProvider applicationServiceProvider)
    {
        optionsBuilder.UseSqlite(Options.Database.ConnectionString);
    }

    private sealed class InMemoryConnectionWrapper : IDisposable, IAsyncDisposable
    {
        public InMemoryConnectionWrapper(DbOptions dbOptions)
        {
            Connection = new SqliteConnection(dbOptions.ConnectionString);
            Connection.Open();
        }

        public void Dispose() => Connection.Dispose();

        public ValueTask DisposeAsync() => Connection.DisposeAsync();

        public SqliteConnection Connection { get; }
    }

    public sealed class Factory : IConfigurationFactory
    {
        public string ProviderName => SqliteProperties.ProviderName;

        public EFCoreConfiguration Create(DataAccessOptions options) => new SqliteConfiguration(options);
    }
}
