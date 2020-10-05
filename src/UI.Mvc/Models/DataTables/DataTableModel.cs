using WebApp.Service;

namespace WebApp.UI.Models.DataTables
{
    public abstract class DataTableModel<TQuery, TResult, TItem>
        where TQuery : ListQuery, IQuery<TResult>
        where TResult: ListResult<TItem>
    {
        public TQuery Query { get; set; } = null!;
        public TResult Result { get; set; } = null!;

        public DataTableDefinition<TItem> ToTableDefinition() => new DataTableDefinition<TItem>(Query, Result);
    }
}
