using System.ComponentModel.DataAnnotations;

namespace WebApp.Service
{
    public interface IListQuery : IQuery, IValidatableObject
    {
        string[]? OrderBy { get; set; }
        bool IsOrdered { get; }

        int PageIndex { get; set; }
        int PageSize { get; set; }
        int MaxPageSize { get; set; }
        bool IsPaged { get; }

        bool SkipTotalItemCount { get; set; }

        void EnsurePaging(int maxPageSize);
    }
}
