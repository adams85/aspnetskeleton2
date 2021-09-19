using System;
using WebApp.Service;
using WebApp.UI.Models.DataTables;

namespace WebApp.UI.Models
{
    public abstract class ListModel<TQuery, TResult, TItem> : IDataTableSource
        where TQuery : ListQuery<TResult>
        where TResult: ListResult<TItem>
    {
        public TQuery Query { get; set; } = null!;
        IListQuery IDataTableSource.Query => Query;

        public TResult Result { get; set; } = null!;
        IListResult IDataTableSource.Result => Result;

        Type IDataTableSource.ItemType => typeof(TItem);
    }
}
