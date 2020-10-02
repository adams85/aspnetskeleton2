using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace WebApp.UI.Helpers
{
    public static class RouteValueDictionaryExtensions
    {
        public static void Merge(this RouteValueDictionary routeValues, IEnumerable<KeyValuePair<string, StringValues>> items)
        {
            foreach (var (key, value) in items)
                routeValues.TryAdd(key, value.Count > 1 ? value.ToArray() : (object)value.ToString());
        }
    }
}
