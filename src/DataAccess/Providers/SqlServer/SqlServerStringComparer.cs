using System;

namespace WebApp.DataAccess.Providers.SqlServer
{
    internal sealed class SqlServerStringComparer : StringComparer
    {
        private readonly StringComparer _stringComparer;

        public SqlServerStringComparer(StringComparer stringComparer)
        {
            _stringComparer = stringComparer;
        }

        // SQL Server ignores trailing whitespace
        private string? Normalize(string? value) => value?.TrimEnd(' ');

        public override int Compare(string x, string y) => _stringComparer.Compare(Normalize(x), Normalize(y));

        public override bool Equals(string x, string y) => _stringComparer.Equals(Normalize(x), Normalize(y));

        public override int GetHashCode(string obj) => _stringComparer.GetHashCode(Normalize(obj) ?? string.Empty);
    }
}
