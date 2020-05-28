using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Infrastructure
{
    public interface IQueryDispatcher
    {
        Task<object?> DispatchAsync(IQuery query, CancellationToken cancellationToken);
        Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken);
    }
}
