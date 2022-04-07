using System;

namespace WebApp.UI.Helpers;

public readonly struct PagerInfo
{
    public PagerInfo(int pageIndex, int pageSize, int totalItemCount)
    {
        PageIndex = pageIndex >= 0 ? pageIndex : throw new ArgumentOutOfRangeException(nameof(pageIndex));
        TotalItemCount = totalItemCount >= 0 ? totalItemCount : throw new ArgumentOutOfRangeException(nameof(totalItemCount));
        PageSize = pageSize > 0 ? pageSize : 0;

        if (pageSize > 0)
        {
            ItemStartIndex = pageIndex * pageSize;
            ItemEndIndex = Math.Min(ItemStartIndex + pageSize, totalItemCount);
            if (TotalItemCount > 0)
            {
                PageCount = Math.DivRem(TotalItemCount, PageSize, out var remainder);
                if (remainder > 0)
                    PageCount++;
            }
            else
                PageCount = 1;
        }
        else
        {
            ItemStartIndex = 0;
            ItemEndIndex = totalItemCount;
            PageCount = 1;
        }
    }

    public int PageIndex { get; }
    public int PageSize { get; }
    public int TotalItemCount { get; }

    public int ItemStartIndex { get; }
    public int ItemEndIndex { get; }

    public int PageNumber => PageIndex + 1;
    public int PageCount { get; }

    public bool IsFirstPage => PageIndex == 0;
    public bool IsLastPage => PageIndex == PageCount - 1;
    public bool HasPreviousPage => PageIndex > 0;
    public bool HasNextPage => PageIndex < PageCount - 1;
}
