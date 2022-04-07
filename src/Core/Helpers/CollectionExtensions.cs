using System;
using System.Collections.Generic;

namespace WebApp.Core.Helpers;

public static class CollectionExtensions
{
    public static void RemoveAll<T>(this IList<T> list, Func<T, int, bool> match)
    {
        for (var i = list.Count - 1; i >= 0; i--)
            if (match(list[i], i))
                list.RemoveAt(i);
    }

    public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<TKey, TValue, bool> match)
    {
        var keysToRemove = new List<TKey>();
        foreach (var (key, value) in dictionary)
            if (match(key, value))
                keysToRemove.Add(key);

        for (var i = keysToRemove.Count - 1; i >= 0; i--)
            dictionary.Remove(keysToRemove[i]);
    }
}
