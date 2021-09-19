using System.Collections.Generic;

namespace WebApp.UI.Infrastructure.Navigation
{
    public interface IPageCollectionProvider
    {
        IEnumerable<PageInfo> GetPages();
    }
}
