using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace CodegenTools
{
    public interface ICommand
    {
        Task<int> OnExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken);
    }
}
