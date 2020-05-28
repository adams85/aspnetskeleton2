using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApp.DataAccess
{
    public class ReadOnlyDataContext : DataContext
    {
        private static InvalidOperationException ChangesNotAllowedError()
        {
            return new InvalidOperationException("This context is read-only, it does not allow changes to the database.");
        }

        public ReadOnlyDataContext(DbContextOptions<ReadOnlyDataContext> options) : base(options)
        {
            // https://www.c-sharpcorner.com/article/no-tracking-with-entity-framework-core/
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public override int SaveChanges()
        {
            throw ChangesNotAllowedError();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            throw ChangesNotAllowedError();
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            throw ChangesNotAllowedError();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw ChangesNotAllowedError();
        }
    }
}
