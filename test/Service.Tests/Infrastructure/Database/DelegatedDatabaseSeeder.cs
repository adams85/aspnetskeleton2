using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess;

namespace WebApp.Service.Tests.Infrastructure.Database
{
    public class DelegatedDatabaseSeeder : IDatabaseSeeder
    {
        private readonly Func<IDbContextFactory<WritableDataContext>, CancellationToken, Task> _seeder;

        public DelegatedDatabaseSeeder(Func<IDbContextFactory<WritableDataContext>, CancellationToken, Task> seeder)
        {
            _seeder = seeder;
        }

        public DelegatedDatabaseSeeder(Func<WritableDataContext, CancellationToken, Task> seeder)
        {
            _seeder = async (ctxFactory, ct) =>
            {
                await using (var dbContext = ctxFactory.CreateDbContext())
                    await seeder(dbContext, ct);
            };
        }

        public Task SeedAsync(IDbContextFactory<WritableDataContext> dbContextFactory, CancellationToken cancellationToken = default) =>
            _seeder(dbContextFactory, cancellationToken);
    }
}
