using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Core.Infrastructure;

namespace Microsoft.Extensions.Hosting
{
    public static class HostExtensions
    {
        public static async Task InitializeApplicationAsync(this IHost host, CancellationToken cancellationToken = default)
        {
            var applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(applicationLifetime.ApplicationStopping, cancellationToken))
            using (var scope = host.Services.CreateScope())
                foreach (var initializer in scope.ServiceProvider.GetRequiredService<IEnumerable<IApplicationInitializer>>())
                    await initializer.InitializeAsync(linkedCts.Token).ConfigureAwait(false);
        }
    }
}
