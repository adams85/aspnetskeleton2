using System;
using CsvHelper.Configuration;

namespace WebApp.Service.Tests.Infrastructure
{
    /// <summary>
    /// Metadata of CSV files containing test data.
    /// </summary>
    public class CsvFile
    {
        public string FilePath { get; set; } = null!;

        public Type EntityType { get; set; } = null!;

        public Action<IReaderConfiguration>? ConfigureReader { get; set; }
    }
}
