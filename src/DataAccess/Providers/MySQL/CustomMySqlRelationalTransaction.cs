using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.MySQL
{
    internal sealed class CustomMySqlRelationalTransaction : MySqlRelationalTransaction, IExtendedDbContextTransaction, ExtendedRelationalTransaction.IBase
    {
        private ExtendedRelationalTransaction.Impl _impl; // must not be read-only (to avoid defensive copies)!!!

        public CustomMySqlRelationalTransaction(IRelationalConnection connection, DbTransaction transaction, Guid transactionId,
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger, bool transactionOwned)
            : base(connection, transaction, transactionId, logger, transactionOwned)
        {
            _impl = new ExtendedRelationalTransaction.Impl(this);
        }

        public void RegisterForCommit(Action callback) => _impl.RegisterForCommit(callback);

        void ExtendedRelationalTransaction.IBase.Commit() => base.Commit();
        public override void Commit() => _impl.Commit();

        Task ExtendedRelationalTransaction.IBase.CommitAsync(CancellationToken cancellationToken) => base.CommitAsync(cancellationToken);
        public override Task CommitAsync(CancellationToken cancellationToken = default) => _impl.CommitAsync(cancellationToken);

        void ExtendedRelationalTransaction.IBase.ClearTransaction() => base.ClearTransaction();
        protected override void ClearTransaction() => _impl.ClearTransaction();

        Task ExtendedRelationalTransaction.IBase.ClearTransactionAsync(CancellationToken cancellationToken) => base.ClearTransactionAsync(cancellationToken);
        protected override Task ClearTransactionAsync(CancellationToken cancellationToken = default) => _impl.ClearTransactionAsync(cancellationToken);
    }
}
