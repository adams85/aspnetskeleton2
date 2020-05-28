using System;
using System.Collections.Generic;

namespace WebApp.Service.Helpers
{
    public static class EnumerableHelper
    {
        public static Dictionary<TKey, TElement> ToDictionarySafe<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer)
            where TKey : notnull
        {
            var result = new Dictionary<TKey, TElement>(comparer);

            foreach (var item in source)
            {
                var key = keySelector(item);
                if (!result.ContainsKey(key))
                    result.Add(key, elementSelector(item));
            }

            return result;
        }

        public static Dictionary<TKey, TElement> ToDictionarySafe<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
            where TKey : notnull
        {
            return source.ToDictionarySafe(keySelector, elementSelector, null);
        }

        public static Dictionary<TKey, TSource> ToDictionarySafe<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
            where TKey : notnull
        {
            return source.ToDictionarySafe(keySelector, item => item, comparer);
        }

        public static Dictionary<TKey, TSource> ToDictionarySafe<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            where TKey : notnull
        {
            return source.ToDictionarySafe(keySelector, null);
        }
    }
}
