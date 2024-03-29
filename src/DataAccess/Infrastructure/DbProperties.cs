﻿using System;
using System.Data;

namespace WebApp.DataAccess.Infrastructure;

public abstract class DbProperties : IDbProperties
{
    protected DbProperties(DbOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        CharacterEncoding = options.CharacterEncoding ?? DefaultCharacterEncoding;

        CaseSensitiveCollation = options.CaseSensitiveCollation ?? DefaultCaseSensitiveCollation;
        CaseSensitiveComparer = CreateCaseSensitiveComparer(CaseSensitiveCollation);

        CaseInsensitiveCollation = options.CaseInsensitiveCollation ?? DefaultCaseInsensitiveCollation;
        CaseInsensitiveComparer = CreateCaseInsensitiveComparer(CaseInsensitiveCollation);
    }

    public abstract string Provider { get; }

    public virtual IsolationLevel SnaphsotIsolationLevel => IsolationLevel.Snapshot;

    protected virtual string? DefaultCharacterEncoding => null;
    public string? CharacterEncoding { get; }

    protected abstract string DefaultCaseSensitiveCollation { get; }
    public string CaseSensitiveCollation { get; }
    public StringComparer CaseSensitiveComparer { get; }

    protected abstract string DefaultCaseInsensitiveCollation { get; }
    public string CaseInsensitiveCollation { get; }
    public StringComparer CaseInsensitiveComparer { get; }

    protected Exception CreateUndefinedCollationError(string provider, string? collation, bool caseSensitive)
    {
        string methodName = this.GetType() + "." + (caseSensitive ? nameof(CreateCaseSensitiveComparer) : nameof(CreateCaseInsensitiveComparer));
        return new InvalidOperationException($"No comparer is defined for the combination of provider {provider} and collation {collation ?? "(default)"}. Modify {methodName} to provide a comparer for the required combination.");
    }

    protected abstract StringComparer CreateCaseSensitiveComparer(string collation);

    protected abstract StringComparer CreateCaseInsensitiveComparer(string collation);
}
