using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Karambolo.Common;

namespace WebApp.Core.Helpers
{
    public static class StringHelper
    {
        private const char CommaEscapeChar = '\\';
        private const string CommaSeparator = ", ";
        private static readonly char[] s_commaSeparatorSpecialChars = new[] { CommaSeparator[0] };

        public static IEnumerable<string> SplitCommaSeparatedList(string value, StringSplitOptions options = StringSplitOptions.None)
        {
            foreach (var item in value.SplitEscaped(CommaEscapeChar, CommaSeparator[0], options))
            {
                var trimmedItem = item.Trim();
                if (trimmedItem.Length > 0 || options != StringSplitOptions.RemoveEmptyEntries)
                    yield return trimmedItem;
            }
        }

        public static string JoinCommaSeparatedList(IEnumerable<string> items) =>
            string.Join(CommaSeparator, items.Select(item => item.Escape(CommaEscapeChar, s_commaSeparatorSpecialChars)));

        public static bool TryNormalizeCommaSeparatedList(string? value, [MaybeNullWhen(false)] out List<string> normalizedItems,
            Func<string, string?>? normalizeItem = null, StringSplitOptions options = StringSplitOptions.None)
        {
            if (value == null)
            {
                normalizedItems = default;
                return false;
            }

            List<string> items;
            normalizeItem ??= Identity<string>.Func;

            using (var enumerator = SplitCommaSeparatedList(value, options).GetEnumerator())
                if (enumerator.MoveNext())
                {
                    var normalizedItem = normalizeItem(enumerator.Current);
                    if (normalizedItem == null)
                    {
                        normalizedItems = default;
                        return false;
                    }

                    items = new List<string>(1);

                    for (; ; )
                    {
                        items.Add(normalizedItem);

                        if (!enumerator.MoveNext())
                            break;

                        normalizedItem = normalizeItem(enumerator.Current);
                        if (normalizedItem == null)
                        {
                            normalizedItems = default;
                            return false;
                        }
                    }
                }
                else
                    items = new List<string>();

            normalizedItems = items;
            return true;
        }
    }
}
