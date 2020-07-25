using System;
using System.Collections.Generic;
using System.Data;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.MySQL
{
    public sealed class MySqlProperties : DbProperties
    {
        public const string ProviderName = "Pomelo.EntityFrameworkCore.MySql";

        public const string DefaultCaseSensitiveCollationName = "utf8mb4_0900_as_cs";
        public const string DefaultCaseInsensitiveCollationName = "utf8mb4_0900_as_ci";

        public MySqlProperties(DbOptions options) : base(options) { }

        public override string Provider => ProviderName;

        // https://stackoverflow.com/questions/9880555/how-to-set-innodb-in-mysql-to-the-snapshot-isolation-level
        public override IsolationLevel SnaphsotIsolationLevel => IsolationLevel.RepeatableRead;

        protected override string DefaultCaseSensitiveCollation => DefaultCaseSensitiveCollationName;
        protected override string DefaultCaseInsensitiveCollation => DefaultCaseInsensitiveCollationName;

        protected override IEqualityComparer<string> CreateCaseSensitiveComparer(string collation)
        {
            return collation switch
            {
                DefaultCaseSensitiveCollationName => StringComparer.Ordinal,
                _ => throw CreateUndefinedCollationError(Provider, CaseSensitiveCollation, caseSensitive: true),
            };
        }

        protected override IEqualityComparer<string> CreateCaseInsensitiveComparer(string collation)
        {
            return collation switch
            {
                DefaultCaseInsensitiveCollationName => StringComparer.OrdinalIgnoreCase,
                _ => throw CreateUndefinedCollationError(Provider, collation, caseSensitive: false),
            };
        }
    }
}
