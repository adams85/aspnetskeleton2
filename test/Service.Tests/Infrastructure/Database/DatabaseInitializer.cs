using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Tests.Infrastructure.Database
{
    public class DatabaseInitializer : ITestInitializer
    {
        private readonly IEnumerable<IDatabaseSeeder> _seeders;

        public DatabaseInitializer(IEnumerable<IDatabaseSeeder> seeders)
        {
            _seeders = seeders;
        }

        public async Task InitializeAsync(TestContext context, CancellationToken cancellationToken = default)
        {
            using (var dbContext = context.CreateWritableDbContext())
                foreach (var seeder in _seeders)
                    await seeder.SeedAsync(dbContext, cancellationToken);
        }
    }
}
