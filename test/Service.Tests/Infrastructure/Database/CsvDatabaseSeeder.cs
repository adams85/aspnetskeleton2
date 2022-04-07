using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess;

namespace WebApp.Service.Tests.Infrastructure.Database;

public class CsvDatabaseSeeder : IDatabaseSeeder
{
    private static readonly CultureInfo s_defaultCsvCulture = CultureInfo.GetCultureInfo("en-US");

    private readonly CsvFile[] _files;
    private readonly Action<CsvConfiguration> _configureReader;
    private readonly Action<CsvContext> _initializeReaderContext;

    public CsvDatabaseSeeder(IEnumerable<CsvFile> files, Action<CsvConfiguration>? configureReader = null, Action<CsvContext>? initializeReaderContext = null)
    {
        _files = files.ToArray();
        _configureReader = configureReader ?? DefaultConfigureReader;
        _initializeReaderContext = initializeReaderContext ?? DefaultInitializeReaderContext;
    }

    private void DefaultConfigureReader(CsvConfiguration configuration)
    {
        configuration.HasHeaderRecord = true;
        configuration.Delimiter = ";";
        configuration.IgnoreReferences = true;
        configuration.HeaderValidated = null;
        configuration.MissingFieldFound = null;
    }

    private void DefaultInitializeReaderContext(CsvContext context)
    {
        // https://github.com/JoshClose/CsvHelper/issues/670
        // HACK: it seems there's no built-in way to apply general settings for automatic type conversions,
        // so we gather the possible types and apply the necessary settings type by type.

        const BindingFlags allPublicInstanceMembers = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance;

        var publicInstanceMemberTypes = _files
            .Select(file => file.EntityType)
            .SelectMany(type =>
                type.GetProperties(allPublicInstanceMembers).Select(property => property.PropertyType).Concat(
                type.GetFields(allPublicInstanceMembers).Select(property => property.FieldType)))
            .Distinct();

        foreach (var type in publicInstanceMemberTypes)
        {
            var options = context.TypeConverterOptionsCache.GetOptions(type);
            options.CultureInfo = context.Configuration.CultureInfo;

            // consider "NULL" string as NULL value
            if (!type.IsValueType || Nullable.GetUnderlyingType(type) != null)
                options.NullValues.Add("NULL");
        }
    }

    public async Task SeedAsync(IDbContextFactory<WritableDataContext> dbContextFactory, CancellationToken cancellationToken = default)
    {
        await using (var dbContext = dbContextFactory.CreateDbContext())
        {
            foreach (var file in _files)
            {
                using (var reader = new StreamReader(file.FilePath))
                using (var csv = new CsvReader(reader, CreateReaderConfiguration(_configureReader, file.ConfigureReader)))
                {
                    _initializeReaderContext(csv.Context);
                    file.InitializeReaderContext?.Invoke(csv.Context);

                    var entities = csv.GetRecords(file.EntityType);

                    dbContext.AddRange(entities);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        static CsvConfiguration CreateReaderConfiguration(Action<CsvConfiguration> applyGlobalConfiguration, Action<CsvConfiguration>? applyFileConfiguration)
        {
            var configuration = new CsvConfiguration(s_defaultCsvCulture);
            applyGlobalConfiguration(configuration);
            applyFileConfiguration?.Invoke(configuration);
            return configuration;
        }
    }
}
