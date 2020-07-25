using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.SqlServer
{
    internal sealed class CustomSqlServerMigrationsSqlGenerator : SqlServerMigrationsSqlGenerator
    {
        private readonly string _caseSensitiveCollation;
        private readonly string _caseInsensitiveCollation;

        public CustomSqlServerMigrationsSqlGenerator(IDbProperties dbProperties, MigrationsSqlGeneratorDependencies dependencies, IMigrationsAnnotationProvider migrationsAnnotations)
            : base(dependencies, migrationsAnnotations)
        {
            if (dbProperties == null)
                throw new ArgumentNullException(nameof(dbProperties));

            _caseSensitiveCollation = dbProperties.CaseSensitiveCollation;
            _caseInsensitiveCollation = dbProperties.CaseInsensitiveCollation;
        }

        // based on: https://github.com/dotnet/efcore/blob/v3.1.6/src/EFCore.SqlServer/Migrations/SqlServerMigrationsSqlGenerator.cs#L777
        protected override void Generate(SqlServerCreateDatabaseOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            base.Generate(operation, model, builder);

            // https://github.com/aspnet/EntityFrameworkCore/issues/6577#issuecomment-475172948
            builder
                .Append("ALTER DATABASE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" COLLATE ")
                .Append(_caseSensitiveCollation)
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: true);
        }

        protected override void ColumnDefinition(string schema, string table, string name, ColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            if (operation.ComputedColumnSql == null)
            {
                var annotation = operation.FindAnnotation(ModelBuilderExtensions.CaseInsensitiveAnnotationKey);
                if (annotation?.Value is bool caseInsensitive)
                {
                    if (operation.ColumnType == null)
                        operation.ColumnType = GetColumnType(schema, table, name, operation, model);

                    operation.ColumnType += " COLLATE " + (caseInsensitive ? _caseInsensitiveCollation : _caseSensitiveCollation);
                }
            }

            base.ColumnDefinition(schema, table, name, operation, model, builder);
        }
    }
}
