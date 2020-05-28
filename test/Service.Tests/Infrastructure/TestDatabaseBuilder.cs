using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using CsvHelper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApp.Core.Infrastructure;
using WebApp.Service.Infrastructure.Database;
using WebApp.Service.Tests.Infrastructure.Database;

namespace WebApp.Service.Tests.Infrastructure
{
    /// <summary>
    /// Builder for test case database configuration.
    /// </summary>
    public class TestDatabaseBuilder
    {
        public TestDatabaseBuilder(TestContextBuilder contextBuilder)
        {
            ContextBuilder = contextBuilder;
        }

        public TestContextBuilder ContextBuilder { get; }

        public TestDatabaseBuilder AddSeeder(Func<IServiceProvider, IDatabaseSeeder> seederFactory)
        {
            ContextBuilder.AddInitializer<DatabaseInitializer>();
            ContextBuilder.Services.AddSingleton(seederFactory);
            return this;
        }

        public TestDatabaseBuilder SeedDefaults()
        {
            return AddSeeder(sp => new DelegatedDatabaseSeeder((ctx, ct) =>
                new DbInitializer(
                    ctx,
                    sp.GetRequiredService<IOptions<DbInitializerOptions>>(),
                    sp.GetService<IClock>(),
                    sp.GetService<ILogger<DbInitializer>>())
                .SeedAsync(ct)));

        }

        public TestDatabaseBuilder SeedDataset(IEnumerable<CsvFile> files, Action<IReaderConfiguration>? configureReader = null)
        {
            return AddSeeder(_ => new CsvDatabaseSeeder(files));
        }
    }
}
