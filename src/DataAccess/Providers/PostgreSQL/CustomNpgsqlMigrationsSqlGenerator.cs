using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Operations;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.PostgreSQL
{
    internal sealed class CustomNpgsqlMigrationsSqlGenerator : NpgsqlMigrationsSqlGenerator
    {
        private readonly string _dbEncoding;
        private readonly string _dbCollation;

        public CustomNpgsqlMigrationsSqlGenerator(IDbProperties dbProperties, MigrationsSqlGeneratorDependencies dependencies, IMigrationsAnnotationProvider migrationsAnnotations, INpgsqlOptions npgsqlOptions)
            : base(dependencies, migrationsAnnotations, npgsqlOptions)
        {
            if (dbProperties == null)
                throw new ArgumentNullException(nameof(dbProperties));

            _dbEncoding = dbProperties.CharacterEncoding ?? NpgsqlProperties.DefaultCharacterEncodingName;
            _dbCollation = dbProperties.CaseSensitiveCollation;
        }

        // based on: https://github.com/npgsql/efcore.pg/blob/v3.1.11/src/EFCore.PG/Migrations/NpgsqlMigrationsSqlGenerator.cs#L731
        protected override void Generate(NpgsqlCreateDatabaseOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
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
                .Append('\'').Append(_dbEncoding).Append('\'');

            builder
                .Append(" LC_COLLATE ")
                .Append('\'').Append(_dbCollation).Append('\'');

            builder
                .Append(" LC_CTYPE ")
                .Append('\'').Append(_dbCollation).Append('\'');

            builder.AppendLine(';');

            EndStatement(builder, suppressTransaction: true);
        }
    }
}
