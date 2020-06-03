using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

        public static async Task<Dictionary<TKey, TElement>> ToDictionarySafeAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default)
            where TKey : notnull
        {
            var result = new Dictionary<TKey, TElement>(comparer);

            // http://blog.monstuff.com/archives/2019/03/async-enumerables-with-cancellation.html
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var key = keySelector(item);
                if (!result.ContainsKey(key))
                    result.Add(key, elementSelector(item));
            }

            return result;
        }

        public static Task<Dictionary<TKey, TElement>> ToDictionarySafeAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, CancellationToken cancellationToken = default)
            where TKey : notnull
        {
            return source.ToDictionarySafeAsync(keySelector, elementSelector, null, cancellationToken);
        }

        public static Task<Dictionary<TKey, TSource>> ToDictionarySafeAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default)
            where TKey : notnull
        {
            return source.ToDictionarySafeAsync(keySelector, item => item, comparer, cancellationToken);
        }

        public static Task<Dictionary<TKey, TSource>> ToDictionarySafeAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default)
            where TKey : notnull
        {
            return source.ToDictionarySafeAsync(keySelector, null, cancellationToken);
        }
    }
}
