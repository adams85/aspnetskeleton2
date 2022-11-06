using System;
using System.Data;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.MySQL;

public sealed class MySqlProperties : DbProperties
{
    public const string ProviderName = "Pomelo.EntityFrameworkCore.MySql";

    public const string DefaultCaseSensitiveCollationName = "utf8mb4_bin";
    public const string DefaultCaseInsensitiveCollationName = "utf8mb4_general_ci";

    public MySqlProperties(DbOptions options) : base(options) { }

    public override string Provider => ProviderName;

    // https://stackoverflow.com/questions/9880555/how-to-set-innodb-in-mysql-to-the-snapshot-isolation-level
    public override IsolationLevel SnaphsotIsolationLevel => IsolationLevel.RepeatableRead;

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
