using System;
using System.Data;

namespace WebApp.DataAccess.Infrastructure
{
    public interface IDbProperties
    {
        string Provider { get; }

        IsolationLevel SnaphsotIsolationLevel { get; }

        string? CharacterEncoding { get; }

        string CaseSensitiveCollation { get; }
        StringComparer CaseSensitiveComparer { get; }

        string CaseInsensitiveCollation { get; }
        StringComparer CaseInsensitiveComparer { get; }
    }
}
