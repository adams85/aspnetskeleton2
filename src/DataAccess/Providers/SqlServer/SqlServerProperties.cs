using System;
using System.Collections.Generic;
using Karambolo.Common;

namespace WebApp.DataAccess.Providers.SqlServer
{
    public sealed class SqlServerProperties : DbProperties
    {
        public const string ProviderName = "Microsoft.EntityFrameworkCore.SqlServer";
        public const string DefaultCaseSensitiveCollationName = "SQL_Latin1_General_CP1_CS_AS";
        public const string DefaultCaseInsensitiveCollationName = "SQL_Latin1_General_CP1_CI_AS";

        public SqlServerProperties(DbOptions options) : base(options) { }

        public override string Provider => ProviderName;

        protected override string DefaultCaseSensitiveCollation => DefaultCaseSensitiveCollationName;
        protected override string DefaultCaseInsensitiveCollation => DefaultCaseInsensitiveCollationName;

        protected override IEqualityComparer<string> CreateCaseSensitiveComparer(string collation)
        {
            return collation switch
            {
                DefaultCaseSensitiveCollationName => DelegatedEqualityComparer.Create<string>(
                    (x, y) => StringComparer.InvariantCulture.Equals(x?.TrimEnd(' '), y?.TrimEnd(' ')),
                    obj => StringComparer.InvariantCulture.GetHashCode(obj?.TrimEnd(' ') ?? string.Empty)),

                _ => throw CreateUndefinedCollationError(Provider, CaseSensitiveCollation, caseSensitive: true),
            };
        }

        protected override IEqualityComparer<string> CreateCaseInsensitiveComparer(string collation)
        {
            return collation switch
            {
                DefaultCaseInsensitiveCollationName => DelegatedEqualityComparer.Create<string>(
                    (x, y) => StringComparer.InvariantCultureIgnoreCase.Equals(x?.TrimEnd(' '), y?.TrimEnd(' ')),
                    obj => StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj?.TrimEnd(' ') ?? string.Empty)),

                _ => throw CreateUndefinedCollationError(Provider, collation, caseSensitive: false),
            };
        }
    }
}
