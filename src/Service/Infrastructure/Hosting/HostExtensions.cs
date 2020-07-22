using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Core.Helpers;
using WebApp.Core.Infrastructure;

namespace Microsoft.Extensions.Hosting
{
    public static class HostExtensions
    {
        internal static async Task InitializeApplicationAsync(this IServiceProvider hostServices, CancellationToken cancellationToken)
        {
            await using (var scope = DisposableAdapter.From(hostServices.CreateScope()))
                foreach (var initializer in scope.Value.ServiceProvider.GetRequiredService<IEnumerable<IApplicationInitializer>>())
                    await initializer.InitializeAsync(cancellationToken).ConfigureAwait(false);
        }

        public static async Task InitializeApplicationAsync(this IHost host, CancellationToken cancellationToken = default)
        {
            var applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(applicationLifetime.ApplicationStopping, cancellationToken))
                await host.Services.InitializeApplicationAsync(linkedCts.Token).ConfigureAwait(false);
        }
    }
}
