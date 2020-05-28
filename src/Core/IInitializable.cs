using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Core
{
    public interface IInitializable
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}
