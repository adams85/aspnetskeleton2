using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Pomelo.EntityFrameworkCore.MySql.Migrations;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.MySQL
{
    internal sealed class CustomMySqlMigrationsSqlGenerator : MySqlMigrationsSqlGenerator
    {
        private readonly string? _dbCharacterSet;
        private readonly string _dbCollation;

        public CustomMySqlMigrationsSqlGenerator(IDbProperties dbProperties, MigrationsSqlGeneratorDependencies dependencies, IMigrationsAnnotationProvider migrationsAnnotations, IMySqlOptions mySqlOptions)
            : base(dependencies, migrationsAnnotations, mySqlOptions)
        {
            if (dbProperties == null)
                throw new ArgumentNullException(nameof(dbProperties));

            _dbCharacterSet = dbProperties.CharacterEncoding;
            _dbCollation = dbProperties.CaseSensitiveCollation;
        }

        // TODO: revise this approach when https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/issues/618 gets resolved
        // based on: https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/blob/3.1.2/src/EFCore.MySql/Migrations/MySqlMigrationsSqlGenerator.cs#L373
        protected override void Generate(MySqlCreateDatabaseOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder
                 .Append("CREATE DATABASE ")
                 .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            // if not specified, the default character set associated with the collation is used
            // https://dev.mysql.com/doc/refman/8.0/en/charset-database.html
            if (_dbCharacterSet != null)
                builder
                    .Append(" CHARACTER SET ")
                    .Append(_dbCharacterSet);

            builder
                .Append(" COLLATE ")
                .Append(_dbCollation);

            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand();
        }
    }
}
