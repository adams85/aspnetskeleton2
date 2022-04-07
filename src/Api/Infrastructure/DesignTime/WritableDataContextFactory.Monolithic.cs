using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using WebApp.DataAccess;
using WebApp.Service.Infrastructure.Database;

namespace WebApp.Api.Infrastructure.DesignTime;

internal class WritableDataContextFactory : IDesignTimeDbContextFactory<WritableDataContext>
{
    public WritableDataContext CreateDbContext(string[] args)
    {
        if (!Enum.TryParse<DbSeedObjects>(Environment.GetEnvironmentVariable("EFCORE_MIGRATIONS_SEED"), out var seed))
            seed = DbSeedObjects.None;

        // we need to override some settings of appsettings.json (this works because environment variables takes precedence in the default setup of Host.CreateDefaultBuilder)
        Environment.SetEnvironmentVariable($"DATABASE__{nameof(DbInitializerOptions.EnsureCreated).ToUpperInvariant()}", bool.FalseString, EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable($"DATABASE__{nameof(DbInitializerOptions.Seed).ToUpperInvariant()}", seed.ToString(), EnvironmentVariableTarget.Process);

        var scope = Program.CreateHostBuilder(args).Build().Services.CreateScope();
        var dbInitializer = ActivatorUtilities.CreateInstance<DbInitializer>(scope.ServiceProvider);

        var context = scope.ServiceProvider.GetRequiredService<IDbContextFactory<WritableDataContext>>().CreateDbContext();
        context.Disposing += (s, e) => dbInitializer.InitializeAsync(designTime: true, default).GetAwaiter().GetResult();

        return context;
    }
}
