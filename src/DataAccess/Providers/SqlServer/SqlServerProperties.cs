using System;
using WebApp.DataAccess.Infrastructure;

namespace WebApp.DataAccess.Providers.SqlServer;

public sealed class SqlServerProperties : DbProperties
{
    public const string ProviderName = "Microsoft.EntityFrameworkCore.SqlServer";
    public const string DefaultCaseSensitiveCollationName = "SQL_Latin1_General_CP1_CS_AS";
    public const string DefaultCaseInsensitiveCollationName = "SQL_Latin1_General_CP1_CI_AS";

    public SqlServerProperties(DbOptions options) : base(options) { }

    public override string Provider => ProviderName;

    protected override string DefaultCaseSensitiveCollation => DefaultCaseSensitiveCollationName;
    protected override string DefaultCaseInsensitiveCollation => DefaultCaseInsensitiveCollationName;

    protected override StringComparer CreateCaseSensitiveComparer(string collation)
    {
        return collation switch
        {
            DefaultCaseSensitiveCollationName => new SqlServerStringComparer(StringComparer.InvariantCulture),
            _ => throw CreateUndefinedCollationError(Provider, CaseSensitiveCollation, caseSensitive: true),
        };
    }

    protected override StringComparer CreateCaseInsensitiveComparer(string collation)
    {
        return collation switch
        {
            DefaultCaseInsensitiveCollationName => new SqlServerStringComparer(StringComparer.InvariantCultureIgnoreCase),
            _ => throw CreateUndefinedCollationError(Provider, collation, caseSensitive: false),
        };
    }
}
