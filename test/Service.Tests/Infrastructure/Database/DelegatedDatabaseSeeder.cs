using System;
using System.Threading;
using System.Threading.Tasks;
using WebApp.DataAccess;

namespace WebApp.Service.Tests.Infrastructure.Database
{
    public class DelegatedDatabaseSeeder : IDatabaseSeeder
    {
        private readonly Func<WritableDataContext, CancellationToken, Task> _seeder;

        public DelegatedDatabaseSeeder(Func<WritableDataContext, CancellationToken, Task> seeder)
        {
            _seeder = seeder;
        }

        public Task SeedAsync(WritableDataContext dbContext, CancellationToken cancellationToken = default) =>
            _seeder(dbContext, cancellationToken);
    }
}
