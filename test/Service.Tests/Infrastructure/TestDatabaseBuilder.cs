using System;
using System.Collections.Generic;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApp.Core.Infrastructure;
using WebApp.DataAccess;
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
            return AddSeeder(sp =>
            {
                var dbInitializer = new DbInitializer(
                    sp.GetRequiredService<IDbContextFactory<WritableDataContext>>(),
                    sp.GetRequiredService<IOptions<DbInitializerOptions>>(),
                    sp.GetService<IClock>(),
                    sp.GetService<ILogger<DbInitializer>>());

                return new DelegatedDatabaseSeeder(dbInitializer.SeedAsync);
            });
        }

        public TestDatabaseBuilder SeedDataset(IEnumerable<CsvFile> files, Action<IReaderConfiguration>? configureReader = null)
        {
            return AddSeeder(_ => new CsvDatabaseSeeder(files));
        }
    }
}
