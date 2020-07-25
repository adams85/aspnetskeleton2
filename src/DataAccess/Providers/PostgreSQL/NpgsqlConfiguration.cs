using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.PostgreSQL
{
    internal sealed class NpgsqlConfiguration : EFCoreConfiguration
    {
        private NpgsqlConfiguration(DataAccessOptions options) : base(options) { }

        protected override void ConfigureInternalServices(IServiceCollection internalServices, IServiceProvider applicationServiceProvider)
        {
            internalServices.AddEntityFrameworkNpgsql();

            ReplaceRelationalTransactionFactory<RelationalTransactionFactory, ExtendedRelationalTransactionFactory>(internalServices)
                .ReplaceLast(ServiceDescriptor.Scoped<INpgsqlRelationalConnection, CustomNpgsqlRelationalConnection>())
                .ReplaceLast(ServiceDescriptor.Scoped<IMigrationsSqlGenerator, CustomNpgsqlMigrationsSqlGenerator>())
                .ReplaceLast(ServiceDescriptor.Singleton<IModelCustomizer, NpgsqlModelCustomizer>())
                .AddSingleton<IDbProperties>(new NpgsqlProperties(Options.Database));
        }

        protected override void ConfigureOptionsCore(DbContextOptionsBuilder optionsBuilder, IServiceProvider internalServiceProvider, IServiceProvider applicationServiceProvider)
        {
            var serverVersion =
                Options.Database.ServerVersion != null ?
                Version.Parse(Options.Database.ServerVersion) :
                null;

            optionsBuilder.UseNpgsql(Options.Database.ConnectionString, options =>
            {
                if (serverVersion != null)
                    options.SetPostgresVersion(serverVersion);
            });
        }

        public sealed class Factory : IConfigurationFactory
        {
            public string ProviderName => NpgsqlProperties.ProviderName;

            public EFCoreConfiguration Create(DataAccessOptions options) => new NpgsqlConfiguration(options);
        }
    }
}
