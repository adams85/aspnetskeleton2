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
using WebApp.DataAccess;

namespace WebApp.Service.Tests.Infrastructure.Database
{
    public class CsvDatabaseSeeder : IDatabaseSeeder
    {
        private static readonly CultureInfo s_defaultCsvCulture = CultureInfo.GetCultureInfo("en-US");

        private readonly CsvFile[] _files;
        private readonly Action<IReaderConfiguration> _configureReader;

        public CsvDatabaseSeeder(IEnumerable<CsvFile> files, Action<IReaderConfiguration>? configureReader = null)
        {
            _files = files.ToArray();
            _configureReader = configureReader ?? DefaultConfigureReader;
        }

        private void DefaultConfigureReader(IReaderConfiguration configuration)
        {
            configuration.HasHeaderRecord = true;
            configuration.Delimiter = ";";
            configuration.IgnoreReferences = true;
            configuration.HeaderValidated = null;
            configuration.MissingFieldFound = null;

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
                var options = configuration.TypeConverterOptionsCache.GetOptions(type);
                options.CultureInfo = configuration.CultureInfo;

                // consider "NULL" string as NULL value
                if (!type.IsValueType || Nullable.GetUnderlyingType(type) != null)
                    options.NullValues.Add("NULL");
            }
        }

        public async Task SeedAsync(WritableDataContext dbContext, CancellationToken cancellationToken = default)
        {
            foreach (var file in _files)
                using (var reader = new StreamReader(file.FilePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(s_defaultCsvCulture)))
                {
                    // applies global configuration
                    _configureReader(csv.Configuration);

                    // applies optional, file level configuration
                    file.ConfigureReader?.Invoke(csv.Configuration);

                    var entities = csv.GetRecords(file.EntityType);

                    dbContext.AddRange(entities);
                }


            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
