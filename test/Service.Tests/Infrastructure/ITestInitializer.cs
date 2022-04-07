using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Tests.Infrastructure;

/// <summary>
/// Interface for test context initializer services.
/// </summary>
public interface ITestInitializer
{
    Task InitializeAsync(TestContext context, CancellationToken cancellationToken = default);
}
