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
                routeValues.TryAdd(key, value);
        }

        public static string GetDefaultViewPath(this RouteValueDictionary routeValues, string viewName)
        {
            if (!routeValues.TryGetValue("controller", out var controller))
                return viewName;

            if (!routeValues.TryGetValue("area", out var area))
                area = null;

            // casting to string is to avoid string.Format, so string interpolation will be done using string.Concat
            return
                area == null ?
                $"~/Views/{(string)controller}/{viewName}.cshtml" :
                $"~/Areas/{(string)area}/Views/{(string)controller}/{viewName}.cshtml";
        }
    }
}
