using System.Collections;

namespace WebApp.Service
{
    public interface IListResult
    {
        IList? Items { get; }
        int TotalItemCount { get; }
        int PageIndex { get; }
        int PageSize { get; }
    }
}
