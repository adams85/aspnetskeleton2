using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace POTools.Helpers;

public static class StringHelper
{
    private static readonly Regex s_matchNewLinesRegex = new Regex(@"\r?\n|\r", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [return: NotNullIfNotNull(nameof(s))]
    public static string? NormalizeNewLines(this string? s)
    {
        return !string.IsNullOrEmpty(s)
            ? s_matchNewLinesRegex.Replace(s, Environment.NewLine)
            : s;
    }
}
