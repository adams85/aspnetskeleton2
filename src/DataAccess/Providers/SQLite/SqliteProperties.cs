﻿using System;
using System.Collections.Generic;
using System.Data;

namespace WebApp.DataAccess.Providers.Sqlite
{
    public sealed class SqliteProperties : DbProperties
    {
        public const string ProviderName = "Microsoft.EntityFrameworkCore.Sqlite";
        public const string InMemoryDataSource = ":memory:";
        public const string DefaultCaseSensitiveCollationName = "INVARIANT_CS";
        public const string DefaultCaseInsensitiveCollationName = "INVARIANT_CI";

        public SqliteProperties(DbOptions options) : base(options) { }

        public override string Provider => ProviderName;

        public override IsolationLevel SnaphsotIsolationLevel => IsolationLevel.Serializable;

        protected override string DefaultCaseSensitiveCollation => DefaultCaseSensitiveCollationName;
        protected override string DefaultCaseInsensitiveCollation => DefaultCaseInsensitiveCollationName;

        protected override IEqualityComparer<string> CreateCaseSensitiveComparer(string collation)
        {
            return collation switch
            {
                DefaultCaseSensitiveCollationName => StringComparer.InvariantCulture,
                _ => throw CreateUndefinedCollationError(Provider, CaseSensitiveCollation, caseSensitive: true),
            };
        }

        protected override IEqualityComparer<string> CreateCaseInsensitiveComparer(string collation)
        {
            return collation switch
            {
                DefaultCaseInsensitiveCollationName => StringComparer.InvariantCultureIgnoreCase,
                _ => throw CreateUndefinedCollationError(Provider, collation, caseSensitive: false),
            };
        }
    }
}
