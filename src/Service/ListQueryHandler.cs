using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using Microsoft.EntityFrameworkCore;
using WebApp.Service.Helpers;

namespace WebApp.Service
{
    internal abstract class ListQueryHandler<TQuery, TResult, T> : QueryHandler<TQuery, TResult>
        where TQuery : ListQuery, IQuery<TResult>
        where TResult : ListResult<T>, new()
    {
        protected ListQueryHandler() { }

        protected virtual IOrderedQueryable<T> ApplyOrderByElement(IQueryable<T> queryable, string keyPropertyPath, bool descending, bool nested)
        {
            try { return nested ? ((IOrderedQueryable<T>)queryable).ThenBy(keyPropertyPath, descending) : queryable.OrderBy(keyPropertyPath, descending); }
            catch { throw new ServiceErrorException(ServiceErrorCode.ParamNotValid, Lambda.MemberPath((TQuery q) => q.OrderBy), keyPropertyPath); }
        }

        protected IQueryable<T> ApplyPagingAndOrdering(TQuery query, IQueryable<T> queryable)
        {
            if (query.IsOrdered)
                queryable = queryable.ApplyOrdering(ApplyOrderByElement, query.OrderBy!);

            if (query.IsPaged)
                queryable = queryable.ApplyPaging(query.PageIndex, query.PageSize, query.MaxPageSize);

            return queryable;
        }

        protected virtual Task<int> CountAsync(IQueryable<T> queryable, CancellationToken cancellationToken)
        {
            return queryable.CountAsync(cancellationToken);
        }

        protected async Task<int> GetTotalItemCountAsync(TQuery query, IQueryable<T> queryable, T[] items, CancellationToken cancellationToken)
        {
            return
                query.SkipTotalItemCount ? -1 :
                query.IsPaged ? await CountAsync(queryable, cancellationToken).ConfigureAwait(false) :
                items.Length;
        }

        protected virtual Task<T[]> ToArrayAsync(IQueryable<T> queryable, CancellationToken cancellationToken)
        {
            return queryable.ToArrayAsync(cancellationToken);
        }

        protected async Task<TResult> ResultAsync(TQuery query, IQueryable<T> queryable, CancellationToken cancellationToken)
        {
            T[] items = await ToArrayAsync(ApplyPagingAndOrdering(query, queryable), cancellationToken).ConfigureAwait(false);

            var totalItemCount = await GetTotalItemCountAsync(query, queryable, items, cancellationToken).ConfigureAwait(false);

            return Result(items, totalItemCount, query.PageIndex, query.PageSize, query.MaxPageSize);
        }

        protected TResult Result(T[] items, int totalItemCount, int pageIndex, int pageSize, int maxPageSize)
        {
            if (pageSize > 0)
                pageSize = QueryableHelper.GetEffectivePageSize(pageSize, maxPageSize);
            else
                pageSize = pageIndex = 0;

            return new TResult
            {
                Items = items,
                TotalItemCount = totalItemCount,
                PageIndex = pageIndex,
                PageSize = pageSize,
            };
        }
    }

    internal abstract class ListQueryHandler<TQuery, T> : ListQueryHandler<TQuery, ListResult<T>, T>
        where TQuery : ListQuery, IQuery<ListResult<T>>
    {
        protected ListQueryHandler() { }
    }
}
