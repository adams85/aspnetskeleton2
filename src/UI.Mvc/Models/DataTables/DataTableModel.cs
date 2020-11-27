using System;
using WebApp.Service;

namespace WebApp.UI.Models.DataTables
{
    public abstract class DataTableModel<TQuery, TResult, TItem> : IDataTableSource
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
