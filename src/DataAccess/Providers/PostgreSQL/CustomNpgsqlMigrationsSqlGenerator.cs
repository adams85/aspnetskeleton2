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
        private readonly string? _characterEncoding;

        public CustomNpgsqlMigrationsSqlGenerator(IDbProperties dbProperties, MigrationsSqlGeneratorDependencies dependencies, INpgsqlOptions npgsqlOptions) : base(dependencies, npgsqlOptions)
        {
            if (dbProperties == null)
                throw new ArgumentNullException(nameof(dbProperties));

            _characterEncoding = dbProperties.CharacterEncoding;
        }

        // based on: https://github.com/npgsql/efcore.pg/blob/v6.0.3/src/EFCore.PG/Migrations/NpgsqlMigrationsSqlGenerator.cs#L906
        protected override void Generate(
            NpgsqlCreateDatabaseOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder
                .Append("CREATE DATABASE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (!string.IsNullOrEmpty(operation.Template))
            {
                builder
                    .AppendLine()
                    .Append("TEMPLATE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Template));
            }

            if (!string.IsNullOrEmpty(operation.Tablespace))
            {
                builder
                    .AppendLine()
                    .Append("TABLESPACE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Tablespace));
            }

            if (!string.IsNullOrEmpty(operation.Collation))
            {
                builder
                    .AppendLine()
                    .Append("LC_COLLATE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Collation));
            }

            if (!string.IsNullOrEmpty(_characterEncoding))
            {
                builder
                    .AppendLine()
                    .Append("ENCODING ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(_characterEncoding));
            }

            builder.AppendLine(";");

            EndStatement(builder, suppressTransaction: true);
        }
    }
}
