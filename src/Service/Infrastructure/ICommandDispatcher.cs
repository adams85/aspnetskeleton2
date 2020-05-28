using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Infrastructure
{
    public interface ICommandDispatcher
    {
        Task DispatchAsync(ICommand command, CancellationToken cancellationToken);
    }
}
