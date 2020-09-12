using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Karambolo.Common;

namespace WebApp.UI.Infrastructure.Navigation
{
    public class PageCatalog : IPageCatalog
    {
        private readonly IReadOnlyDictionary<string, PageInfo> _pages;

        public PageCatalog(IEnumerable<IPageCollectionProvider> providers)
        {
            _pages = providers
                .SelectMany(provider => provider.GetPages())
                .ToDictionary(page => page.RouteName, Identity<PageInfo>.Func);
        }

        public PageInfo? GetPage(string routeName, bool throwIfNotFound = false) =>
            _pages.TryGetValue(routeName, out var page) ? page :
            !throwIfNotFound ? (PageInfo?)null :
            throw new ArgumentException($"No {nameof(PageInfo)} was found for route {routeName}.", nameof(routeName));

        public IEnumerator<PageInfo> GetEnumerator() => _pages.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
