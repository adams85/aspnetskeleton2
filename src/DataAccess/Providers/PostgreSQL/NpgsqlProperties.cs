﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace WebApp.DataAccess.Providers.PostgreSQL
{
    public sealed class NpgsqlProperties : DbProperties
    {
        public const string ProviderName = "Npgsql.EntityFrameworkCore.PostgreSQL";

        // changing default locale in PostgreSQL:
        // https://gist.github.com/ffmike/877447

        // /* Allow modification of template0 */
        // UPDATE pg_database SET DATALLOWCONN = TRUE WHERE DATNAME = 'template0';

        // /* Create template1 with the correct encoding */
        // \c template0
        // UPDATE pg_database SET DATISTEMPLATE = FALSE WHERE DATNAME = 'template1';
        // DROP DATABASE IF EXISTS template1;
        // CREATE DATABASE template1 WITH TEMPLATE = template0 ENCODING = 'UTF8' LC_CTYPE 'C' LC_COLLATE 'C';
        // UPDATE pg_database SET DATISTEMPLATE = TRUE WHERE DATNAME = 'template1';

        // /* Drop then re-create template0 with the correct encoding */
        // \c template1
        // UPDATE pg_database SET DATISTEMPLATE = FALSE WHERE DATNAME = 'template0';
        // DROP DATABASE template0;
        // CREATE DATABASE template0 WITH TEMPLATE = template1 ENCODING = 'UTF8' LC_CTYPE 'C' LC_COLLATE 'C';
        // UPDATE pg_database SET DATISTEMPLATE = TRUE WHERE DATNAME = 'template0';

        public const string DefaultCaseSensitiveCollationName = "C";
        public const string DefaultCaseInsensitiveCollationName = DefaultCaseSensitiveCollationName;

        public NpgsqlProperties(DbOptions options) : base(options) { }

        public override string Provider => ProviderName;

        // https://mbukowicz.github.io/databases/2020/05/01/snapshot-isolation-in-postgresql.html
        public override IsolationLevel SnaphsotIsolationLevel => IsolationLevel.RepeatableRead;

        protected override string DefaultCaseSensitiveCollation => DefaultCaseSensitiveCollationName;
        protected override string DefaultCaseInsensitiveCollation => DefaultCaseInsensitiveCollationName;

        protected override IEqualityComparer<string> CreateCaseSensitiveComparer(string collation)
        {
            return collation switch
            {
                DefaultCaseSensitiveCollationName => StringComparer.Ordinal,
                "English_United States.1252" => StringComparer.Create(new CultureInfo("en-US"), ignoreCase: false),
                _ => throw CreateUndefinedCollationError(Provider, CaseSensitiveCollation, caseSensitive: true),
            };
        }

        protected override IEqualityComparer<string> CreateCaseInsensitiveComparer(string collation)
        {
            return collation switch
            {
                DefaultCaseInsensitiveCollationName => StringComparer.OrdinalIgnoreCase,
                "English_United States.1252" => StringComparer.Create(new CultureInfo("en-US"), ignoreCase: true),
                _ => throw CreateUndefinedCollationError(Provider, collation, caseSensitive: false),
            };
        }
    }
}
