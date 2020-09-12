using System.Collections.Generic;

namespace WebApp.UI.Infrastructure.Navigation
{
    public interface IPageCatalog : IEnumerable<PageInfo>
    {
        PageInfo? GetPage(string routeName, bool throwIfNotFound = false);
    }
}
