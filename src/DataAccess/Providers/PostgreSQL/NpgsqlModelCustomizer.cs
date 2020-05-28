using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace WebApp.DataAccess.Providers.PostgreSQL
{
    internal sealed class NpgsqlModelCustomizer : RelationalModelCustomizer
    {
        private readonly Version? _postgresVersion;

        public NpgsqlModelCustomizer(ModelCustomizerDependencies dependencies, INpgsqlOptions npgsqlOptions) : base(dependencies)
        {
            _postgresVersion = npgsqlOptions.PostgresVersion;
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);

            // enabling case insensitive texts:
            // https://github.com/npgsql/efcore.pg/issues/406#issuecomment-561367654

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                foreach (var property in entityType.GetProperties())
                    if (property.PropertyInfo != null)
                    {
                        if (Type.GetTypeCode(property.ClrType) == TypeCode.String && property.GetColumnType() == null)
                            property.SetColumnType("citext");
                    }

            // NOTE: prior to v9.1 citext must be enabled manually (preferably, on template1, otherwise the initial migration will fail)
            if (_postgresVersion == null || _postgresVersion >= new Version(9, 1))
                modelBuilder.HasPostgresExtension("citext");
        }
    }
}
