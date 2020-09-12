using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;

namespace WebApp.UI.Infrastructure.Navigation
{
    public static class PageCatalogExtensions
    {
        private static readonly ConcurrentDictionary<MethodInfo, string[]> s_routeNameCache = new ConcurrentDictionary<MethodInfo, string[]>();

        public static PageInfo? FindPage(this IPageCatalog pageCatalog, ActionDescriptor actionDescriptor)
        {
            if (!(actionDescriptor is ControllerActionDescriptor controllerActionDescriptor))
                return null;

            var routeNames = s_routeNameCache.GetOrAdd(controllerActionDescriptor.MethodInfo, method =>
                method.GetCustomAttributes<Attribute>()
                    .OfType<IRouteTemplateProvider>()
                    .Select(provider => provider.Name)
                    .ToArray());

            string routeName;
            PageInfo? page;
            for (int i = 0, n = routeNames.Length; i < n; i++)
                if ((routeName = routeNames[i]) != null && (page = pageCatalog.GetPage(routeName)) != null)
                    return page;

            return null;
        }
    }
}
