using System;
using System.Collections.Generic;

namespace WebApp.Core.Helpers
{
    public static class ListExtensions
    {
        public static void RemoveAll<T>(this IList<T> list, Predicate<T> match)
        {
            for (var i = list.Count - 1; i >= 0; i--)
                if (match(list[i]))
                    list.RemoveAt(i);
        }

        public static void RemoveAll<T>(this IList<T> list, Func<T, int, bool> match)
        {
            for (var i = list.Count - 1; i >= 0; i--)
                if (match(list[i], i))
                    list.RemoveAt(i);
        }
    }
}
