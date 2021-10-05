using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    // based on: https://github.com/dotnet/efcore/blob/v6.0.0-rc.1.21452.10/src/EFCore/Infrastructure/PooledDbContextFactory.cs
    // TODO: this type will become unnecessary and should be removed after upgrading to .NET 5+
    public class PooledDbContextFactory<TContext> : IDbContextFactory<TContext>
        where TContext : PooledDbContext
    {
        public const int DefaultPoolSize = 1024;

        internal static void SetMaxPoolSize(DbContextOptionsBuilder<TContext> optionsBuilder, int poolSize)
        {
            var extension = (optionsBuilder.Options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension())
                .WithMaxPoolSize(poolSize);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        }

        private readonly DbContextPool<TContext> _pool;

        public PooledDbContextFactory(DbContextPool<TContext> pool)
            => _pool = pool;

        public PooledDbContextFactory(DbContextOptions<TContext> options, int poolSize = DefaultPoolSize)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TContext>(options);

            SetMaxPoolSize(optionsBuilder, poolSize);

            _pool = new DbContextPool<TContext>(optionsBuilder.Options);
        }

        public virtual TContext CreateDbContext()
        {
            var lease = new DbContextPool<TContext>.Lease(_pool);
            lease.Context.SetLease(lease);

            return lease.Context;
        }
    }
}
