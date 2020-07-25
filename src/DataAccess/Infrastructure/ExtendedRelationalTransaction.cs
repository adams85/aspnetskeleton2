using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace WebApp.DataAccess.Infrastructure
{
    internal sealed class ExtendedRelationalTransaction : RelationalTransaction, IExtendedDbContextTransaction, ExtendedRelationalTransaction.IBase
    {
        private Impl _impl; // must not be read-only (to avoid defensive copies)!!!

        public ExtendedRelationalTransaction(IRelationalConnection connection, DbTransaction transaction, Guid transactionId,
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger, bool transactionOwned)
            : base(connection, transaction, transactionId, logger, transactionOwned)
        {
            _impl = new Impl(this);
        }

        public void RegisterForCommit(Action callback) => _impl.RegisterForCommit(callback);

        void IBase.Commit() => base.Commit();
        public override void Commit() => _impl.Commit();

        Task IBase.CommitAsync(CancellationToken cancellationToken) => base.CommitAsync(cancellationToken);
        public override Task CommitAsync(CancellationToken cancellationToken = default) => _impl.CommitAsync(cancellationToken);

        void IBase.ClearTransaction() => base.ClearTransaction();
        protected override void ClearTransaction() => _impl.ClearTransaction();

        Task IBase.ClearTransactionAsync(CancellationToken cancellationToken) => base.ClearTransactionAsync(cancellationToken);
        protected override Task ClearTransactionAsync(CancellationToken cancellationToken = default) => _impl.ClearTransactionAsync(cancellationToken);

        #region Support for "multiple inheritance"

        internal interface IBase
        {
            void Commit();
            Task CommitAsync(CancellationToken cancellationToken);

            void ClearTransaction();
            Task ClearTransactionAsync(CancellationToken cancellationToken);
        }

        internal struct Impl : IBase
        {
            private readonly IBase _base;
            private Action? _onCommit;

            public Impl(IBase @base) => (_base, _onCommit) = (@base, null);

            public void RegisterForCommit(Action callback) => _onCommit += callback;

            public void Commit()
            {
                var onCommit = _onCommit;

                _base.Commit();

                onCommit?.Invoke();
            }

            public async Task CommitAsync(CancellationToken cancellationToken)
            {
                var onCommit = _onCommit;

                await _base.CommitAsync(cancellationToken).ConfigureAwait(false);

                onCommit?.Invoke();
            }

            public void ClearTransaction()
            {
                _onCommit = null;

                _base.ClearTransaction();
            }

            public Task ClearTransactionAsync(CancellationToken cancellationToken)
            {
                _onCommit = null;

                return _base.ClearTransactionAsync(cancellationToken);
            }
        }

        #endregion
    }
}
