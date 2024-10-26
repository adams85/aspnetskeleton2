using System;
using WebApp.Service;
using WebApp.UI.Areas.Dashboard.Models.DataTables;
using WebApp.UI.Models;

namespace WebApp.UI.Areas.Dashboard.Models;

public abstract class ListPageModel<TPageDescriptor, TQuery, TResult, TItem> : DashboardPageModel<TPageDescriptor>, IDataTableSource
    where TPageDescriptor : PageDescriptor, new()
    where TQuery : ListQuery<TResult>
    where TResult : ListResult<TItem>
{
    public TQuery Query { get; protected set; } = null!;
    IListQuery IDataTableSource.Query => Query;

    public TResult Result { get; protected set; } = null!;
    IListResult IDataTableSource.Result => Result;

    Type IDataTableSource.ItemType => typeof(TItem);
}
