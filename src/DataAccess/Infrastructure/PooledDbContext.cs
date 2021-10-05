using System;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    // TODO: this type will become unnecessary and should be removed after upgrading to .NET 5+
    public class PooledDbContext : DbContext
    {
        private IDisposable? _lease;

        public PooledDbContext(DbContextOptions options) : base(options) { }

        internal void SetLease(IDisposable lease)
        {
            _lease = lease;
        }

        private bool TryDisposeLease()
        {
            var lease = _lease;
            _lease = null;

            if (lease == null)
                return false;

            lease.Dispose();
            return true;
        }

        public override void Dispose()
        {
            if (!TryDisposeLease())
                base.Dispose();
        }

        public override ValueTask DisposeAsync()
        {
            if (!TryDisposeLease())
                return base.DisposeAsync();

            return default;
        }
    }
}
