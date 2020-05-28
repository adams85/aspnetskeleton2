using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp.DataAccess
{
    public static class DbContextExtensions
    {
        public static IDbProperties GetDbProperties(this DbContext context)
        {
            return context.GetService<IDbProperties>();
        }

        public static ISqlGenerationHelper GetSqlGenerationHelper(this DbContext context)
        {
            return context.GetService<ISqlGenerationHelper>();
        }

        private static IReadOnlyList<MigrationCommand> GenerateMigrationCommands(this DatabaseFacade database, IReadOnlyList<MigrationOperation> operations, IModel model)
        {
            var sqlGenerator = database.GetService<IMigrationsSqlGenerator>();
            return sqlGenerator.Generate(operations, model);
        }

        public static IReadOnlyList<MigrationCommand> GenerateMigrationCommands(this DatabaseFacade database, IEnumerable<MigrationOperation> operations)
        {
            var model = database.GetService<IModel>();
            return database.GenerateMigrationCommands(operations.ToArray(), model);
        }

        public static IReadOnlyList<MigrationCommand> GenerateAllMigrationCommands(this DatabaseFacade database)
        {
            var model = database.GetService<IModel>();
            var modelDiffer = database.GetService<IMigrationsModelDiffer>();
            var operations = modelDiffer.GetDifferences(null, model);
            return database.GenerateMigrationCommands(operations.ToArray(), model);
        }

        public static Task ExecuteMigrationCommandsAsync(this DatabaseFacade database, IEnumerable<MigrationCommand> commands, CancellationToken cancellationToken)
        {
            var commandExecutor = database.GetService<IMigrationCommandExecutor>();
            var connection = database.GetService<IRelationalConnection>();
            return commandExecutor.ExecuteNonQueryAsync(commands, connection, cancellationToken);
        }
    }
}
