using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WebApp.DataAccess.Providers.SqlServer
{
    internal sealed class SqlServerConfiguration : EFCoreConfiguration
    {
        private SqlServerConfiguration(DataAccessOptions options) : base(options) { }

        protected override void ConfigureInternalServices(IServiceCollection internalServices, IServiceProvider applicationServiceProvider) =>
            internalServices
                .AddEntityFrameworkSqlServer()
                .Replace(ServiceDescriptor.Singleton<IMigrationsAnnotationProvider, CustomSqlServerMigrationsAnnotationProvider>())
                .Replace(ServiceDescriptor.Scoped<IMigrationsSqlGenerator, CustomSqlServerMigrationsSqlGenerator>())
                .Replace(ServiceDescriptor.Singleton<IModelCustomizer, SqlServerModelCustomizer>())
                .AddSingleton<IDbProperties>(new SqlServerProperties(Options.Database));

        protected override void ConfigureOptionsCore(DbContextOptionsBuilder optionsBuilder, IServiceProvider internalServiceProvider, IServiceProvider applicationServiceProvider)
        {
            optionsBuilder.UseSqlServer(Options.Database.ConnectionString);
        }

        public sealed class Factory : IConfigurationFactory
        {
            public string ProviderName => SqlServerProperties.ProviderName;

            public EFCoreConfiguration Create(DataAccessOptions options) => new SqlServerConfiguration(options);
        }
    }
}
