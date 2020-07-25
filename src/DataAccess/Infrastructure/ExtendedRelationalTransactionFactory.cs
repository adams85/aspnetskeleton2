using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace WebApp.DataAccess.Infrastructure
{
    internal sealed class ExtendedRelationalTransactionFactory : RelationalTransactionFactory
    {
        public ExtendedRelationalTransactionFactory(RelationalTransactionFactoryDependencies dependencies) : base(dependencies) { }

        public override RelationalTransaction Create(IRelationalConnection connection, DbTransaction transaction, Guid transactionId,
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger, bool transactionOwned) =>
            new ExtendedRelationalTransaction(connection, transaction, transactionId, logger, transactionOwned);
    }
}
