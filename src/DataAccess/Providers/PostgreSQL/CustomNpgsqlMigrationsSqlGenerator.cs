using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Operations;

namespace WebApp.DataAccess.Providers.PostgreSQL
{
    internal sealed class CustomNpgsqlMigrationsSqlGenerator : NpgsqlMigrationsSqlGenerator
    {
        private readonly string _collation;

        public CustomNpgsqlMigrationsSqlGenerator(IDbProperties dbProperties, MigrationsSqlGeneratorDependencies dependencies, IMigrationsAnnotationProvider migrationsAnnotations, INpgsqlOptions npgsqlOptions)
            : base(dependencies, migrationsAnnotations, npgsqlOptions)
        {
            if (dbProperties == null)
                throw new ArgumentNullException(nameof(dbProperties));

            _collation = dbProperties.CaseSensitiveCollation;
        }

        protected override void Generate(NpgsqlCreateDatabaseOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            // original code: https://github.com/npgsql/efcore.pg/blob/v3.1.3/src/EFCore.PG/Migrations/NpgsqlMigrationsSqlGenerator.cs#L731

            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder
                .Append("CREATE DATABASE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (operation.Template != null)
            {
                builder
                    .Append(" TEMPLATE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Template));
            }

            if (operation.Tablespace != null)
            {
                builder
                    .Append(" TABLESPACE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Tablespace));
            }

            builder
                .Append(" ENCODING ")
                .Append('\'').Append("UTF8").Append('\'');

            builder
                .Append(" LC_COLLATE ")
                .Append('\'').Append(_collation).Append('\'');

            builder
                .Append(" LC_CTYPE ")
                .Append('\'').Append(_collation).Append('\'');

            builder.AppendLine(';');

            EndStatement(builder, suppressTransaction: true);
        }
    }
}
