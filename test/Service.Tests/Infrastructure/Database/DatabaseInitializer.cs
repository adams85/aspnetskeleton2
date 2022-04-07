using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApp.DataAccess;

namespace WebApp.Service.Tests.Infrastructure.Database;

public class DatabaseInitializer : ITestInitializer
{
    private readonly IEnumerable<IDatabaseSeeder> _seeders;

    public DatabaseInitializer(IEnumerable<IDatabaseSeeder> seeders)
    {
        _seeders = seeders;
    }

    public async Task InitializeAsync(TestContext context, CancellationToken cancellationToken = default)
    {
        var dbContextFactory = context.Services.GetRequiredService<IDbContextFactory<WritableDataContext>>();

        foreach (var seeder in _seeders)
            await seeder.SeedAsync(dbContextFactory, cancellationToken);
    }
}
