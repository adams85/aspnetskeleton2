using System;
using System.Data;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.Sqlite;

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

    protected override StringComparer CreateCaseSensitiveComparer(string collation)
    {
        return collation switch
        {
            DefaultCaseSensitiveCollationName => StringComparer.InvariantCulture,
            _ => throw CreateUndefinedCollationError(Provider, CaseSensitiveCollation, caseSensitive: true),
        };
    }

    protected override StringComparer CreateCaseInsensitiveComparer(string collation)
    {
        return collation switch
        {
            DefaultCaseInsensitiveCollationName => StringComparer.InvariantCultureIgnoreCase,
            _ => throw CreateUndefinedCollationError(Provider, collation, caseSensitive: false),
        };
    }
}
