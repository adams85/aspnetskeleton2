using WebApp.Service;

namespace WebApp.UI.Models
{
    public abstract class TableModel<TQuery, TResult, TItem>
        where TQuery : ListQuery, IQuery<TResult>
        where TResult: ListResult<TItem>
    {
        public TQuery Query { get; set; } = null!;
        public TResult Result { get; set; } = null!;
    }
}
