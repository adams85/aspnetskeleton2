using System.Collections.Generic;
using System.Data;

namespace WebApp.DataAccess.Infrastructure
{
    public interface IDbProperties
    {
        string Provider { get; }

        IsolationLevel SnaphsotIsolationLevel { get; }

        string? CharacterEncoding { get; }

        string CaseSensitiveCollation { get; }
        IEqualityComparer<string> CaseSensitiveComparer { get; }

        string CaseInsensitiveCollation { get; }
        IEqualityComparer<string> CaseInsensitiveComparer { get; }
    }
}
