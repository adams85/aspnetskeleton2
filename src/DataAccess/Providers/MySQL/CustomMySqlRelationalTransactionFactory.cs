using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;

namespace WebApp.DataAccess.Providers.MySQL
{
    internal sealed class CustomMySqlRelationalTransactionFactory : MySqlRelationalTransactionFactory
    {
        public CustomMySqlRelationalTransactionFactory(RelationalTransactionFactoryDependencies dependencies)
            : base(dependencies) { }

        public override RelationalTransaction Create(IRelationalConnection connection, DbTransaction transaction, Guid transactionId,
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger, bool transactionOwned) =>
            new CustomMySqlRelationalTransaction(connection, transaction, transactionId, logger, transactionOwned);
    }
}
