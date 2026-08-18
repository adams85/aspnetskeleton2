using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace POTools.Helpers;

public static partial class StringHelper
{
    [GeneratedRegex(@"\r?\n|\r", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MatchNewLinesRegex { get; }

    [return: NotNullIfNotNull(nameof(s))]
    public static string? NormalizeNewLines(this string? s)
    {
        return !string.IsNullOrEmpty(s)
            ? MatchNewLinesRegex.Replace(s, Environment.NewLine)
            : s;
    }
}
