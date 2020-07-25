using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace WebApp.DataAccess.Infrastructure
{
    internal sealed class ExtendedRelationalTransaction : RelationalTransaction, IExtendedDbContextTransaction
    {
        private Action? _onCommit;

        public ExtendedRelationalTransaction(IRelationalConnection connection, DbTransaction transaction, Guid transactionId,
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger, bool transactionOwned)
            : base(connection, transaction, transactionId, logger, transactionOwned) { }

        public void RegisterForCommit(Action callback) => _onCommit += callback;

        public override void Commit()
        {
            var onCommitted = _onCommit;

            base.Commit();

            onCommitted?.Invoke();
        }

        public override async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            var onCommitted = _onCommit;

            await base.CommitAsync(cancellationToken).ConfigureAwait(false);

            onCommitted?.Invoke();
        }

        protected override void ClearTransaction()
        {
            _onCommit = null;

            base.ClearTransaction();
        }

        protected override Task ClearTransactionAsync(CancellationToken cancellationToken = default)
        {
            _onCommit = null;

            return base.ClearTransactionAsync(cancellationToken);
        }
    }
}
