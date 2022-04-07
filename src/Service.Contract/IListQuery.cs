using System.ComponentModel.DataAnnotations;

namespace WebApp.Service
{
    public interface IListQuery : IQuery, IValidatableObject
    {
        string[]? OrderBy { get; init; }
        bool IsOrdered { get; }

        int PageIndex { get; init; }
        int PageSize { get; init; }
        int MaxPageSize { get; init; }
        bool IsPaged { get; }

        bool SkipTotalItemCount { get; init; }

        void EnsurePaging(int maxPageSize);
    }
}
