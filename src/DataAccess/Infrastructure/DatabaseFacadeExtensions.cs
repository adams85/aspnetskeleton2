using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using WebApp.DataAccess.Infrastructure;

namespace Microsoft.EntityFrameworkCore
{
    public static class DatabaseFacadeExtensions
    {
        private static bool HasPendingTransaction(this DatabaseFacade database, out Transaction? activeTransaction)
        {
            IDatabaseFacadeDependenciesAccessor dependenciesAccessor = database;
            var transactionManager = dependenciesAccessor.Dependencies.TransactionManager as IExtendedDbContextTransactionManager;
            Debug.Assert(transactionManager != null, $"{nameof(IDbContextTransactionManager)} of provider {database.ProviderName} does not implement {typeof(IExtendedDbContextTransactionManager)}.");

            if (database.CurrentTransaction != null)
            {
                activeTransaction = null;
                return true;
            }

            if (transactionManager.SupportsAmbientTransactions)
            {
                activeTransaction = (transactionManager is ITransactionEnlistmentManager ? database.GetEnlistedTransaction() : null) ?? Transaction.Current;
                if (activeTransaction != null && activeTransaction.TransactionInformation.Status == TransactionStatus.Active)
                    return true;
            }

            activeTransaction = null;
            return false;
        }

        public static bool HasPendingTransaction(this DatabaseFacade database) => database.HasPendingTransaction(out var _);

        public static Task<IDbContextTransaction?> TryBeginTransactionAsync(this DatabaseFacade database, CancellationToken cancellationToken = default) =>
            !database.HasPendingTransaction() ? database.BeginTransactionAsync(cancellationToken) : Task.FromResult<IDbContextTransaction?>(null);

        public static Task<IDbContextTransaction?> TryBeginTransactionAsync(this DatabaseFacade database, System.Data.IsolationLevel isolationLevel, CancellationToken cancellationToken = default) =>
            !database.HasPendingTransaction() ? database.BeginTransactionAsync(isolationLevel, cancellationToken) : Task.FromResult<IDbContextTransaction?>(null);

        public static Task<IDbContextTransaction?> TryUseTransactionAsync(this DatabaseFacade database, DbTransaction transaction, CancellationToken cancellationToken = default) =>
            !database.HasPendingTransaction() ? database.UseTransactionAsync(transaction, cancellationToken) : Task.FromResult<IDbContextTransaction?>(null);

        public static bool TryRegisterForPendingTransactionCommit(this DatabaseFacade database, Action callback)
        {
            if (!database.HasPendingTransaction(out var activeTransaction))
                return false;

            if (activeTransaction != null)
            {
                void TransactionCompletedHandler(object _, TransactionEventArgs e)
                {
                    e.Transaction.TransactionCompleted -= TransactionCompletedHandler;

                    if (e.Transaction.TransactionInformation.Status == TransactionStatus.Committed)
                        callback();
                }

                activeTransaction.TransactionCompleted += TransactionCompletedHandler;
            }
            else
            {
                var dbContextTransaction = database.CurrentTransaction as IExtendedDbContextTransaction;
                Debug.Assert(dbContextTransaction != null, $"The current transaction does not implement {typeof(IExtendedDbContextTransaction)}.");

                dbContextTransaction.RegisterForCommit(callback);
            }

            return true;
        }

        private static IReadOnlyList<MigrationCommand> GenerateMigrationCommands(this DatabaseFacade database, IReadOnlyList<MigrationOperation> operations, IModel model)
        {
            var sqlGenerator = database.GetInfrastructure().GetRequiredService<IMigrationsSqlGenerator>();
            return sqlGenerator.Generate(operations, model);
        }

        public static IReadOnlyList<MigrationCommand> GenerateMigrationCommands(this DatabaseFacade database, IEnumerable<MigrationOperation> operations)
        {
            IDatabaseFacadeDependenciesAccessor dependenciesAccessor = database;
            var model = dependenciesAccessor.Context.Model;
            return database.GenerateMigrationCommands(operations.ToArray(), model);
        }

        public static IReadOnlyList<MigrationCommand> GenerateAllMigrationCommands(this DatabaseFacade database)
        {
            var modelDiffer = database.GetInfrastructure().GetRequiredService<IMigrationsModelDiffer>();
            IDatabaseFacadeDependenciesAccessor dependenciesAccessor = database;
            var model = dependenciesAccessor.Context.Model;
            var operations = modelDiffer.GetDifferences(null, model);
            return database.GenerateMigrationCommands(operations.ToArray(), model);
        }

        public static Task ExecuteMigrationCommandsAsync(this DatabaseFacade database, IEnumerable<MigrationCommand> commands, CancellationToken cancellationToken)
        {
            var infrastructure = database.GetInfrastructure();
            var commandExecutor = infrastructure.GetRequiredService<IMigrationCommandExecutor>();
            var connection = infrastructure.GetRequiredService<IRelationalConnection>();
            return commandExecutor.ExecuteNonQueryAsync(commands, connection, cancellationToken);
        }
    }
}
