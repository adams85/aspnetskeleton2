using System;
using System.Data;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.PostgreSQL
{
    public sealed class NpgsqlProperties : DbProperties
    {
        public const string ProviderName = "Npgsql.EntityFrameworkCore.PostgreSQL";

        // https://www.postgresql.org/docs/current/collation.html
        // http://www.unicode.org/reports/tr35/tr35-collation.html#Setting_Options
        // https://unicode-org.github.io/icu/userguide/collation/concepts.html
        public const string DefaultCaseSensitiveCollationName = "und";
        public const string DefaultCaseInsensitiveCollationName = "und-u-ks-level2";

        public NpgsqlProperties(DbOptions options) : base(options) { }

        public override string Provider => ProviderName;

        // https://mbukowicz.github.io/databases/2020/05/01/snapshot-isolation-in-postgresql.html
        public override IsolationLevel SnaphsotIsolationLevel => IsolationLevel.RepeatableRead;

        protected override string DefaultCaseSensitiveCollation => DefaultCaseSensitiveCollationName;
        protected override string DefaultCaseInsensitiveCollation => DefaultCaseInsensitiveCollationName;

        protected override StringComparer CreateCaseSensitiveComparer(string collation) => collation switch
        {
            DefaultCaseSensitiveCollationName => StringComparer.InvariantCulture,
            _ => throw CreateUndefinedCollationError(Provider, CaseSensitiveCollation, caseSensitive: true),
        };

        protected override StringComparer CreateCaseInsensitiveComparer(string collation) => collation switch
        {
            DefaultCaseInsensitiveCollationName => StringComparer.InvariantCultureIgnoreCase,
            _ => throw CreateUndefinedCollationError(Provider, collation, caseSensitive: false),
        };
    }
}
