using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.MySQL
{
    internal sealed class MySqlConfiguration : EFCoreConfiguration
    {
        private MySqlConfiguration(DataAccessOptions options) : base(options) { }

        protected override void ConfigureInternalServices(IServiceCollection internalServices, IServiceProvider applicationServiceProvider)
        {
            internalServices.AddEntityFrameworkMySql();

            internalServices
                .ReplaceLast(ServiceDescriptor.Scoped<IMySqlRelationalConnection, CustomMySqlRelationalConnection>())
                .ReplaceLast(ServiceDescriptor.Singleton<IModelCustomizer, MySqlModelCustomizer>())
                .AddSingleton<IDbProperties>(new MySqlProperties(Options.Database));
        }

        protected override void ConfigureOptionsCore(DbContextOptionsBuilder optionsBuilder, IServiceProvider internalServiceProvider, IServiceProvider applicationServiceProvider)
        {
            var serverVersion = ServerVersion.Parse(Options.Database.ServerVersion);

            optionsBuilder.UseMySql(Options.Database.ConnectionString, serverVersion);
        }

        public sealed class Factory : IConfigurationFactory
        {
            public string ProviderName => MySqlProperties.ProviderName;

            public EFCoreConfiguration Create(DataAccessOptions options) => new MySqlConfiguration(options);
        }
    }
}
