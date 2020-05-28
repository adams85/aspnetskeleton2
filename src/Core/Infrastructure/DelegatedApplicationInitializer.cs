using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Core.Infrastructure
{
    public sealed class DelegatedApplicationInitializer : IApplicationInitializer
    {
        private readonly Func<CancellationToken, Task> _initializer;

        public DelegatedApplicationInitializer(Func<CancellationToken, Task> initializer)
        {
            _initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
        }

        public DelegatedApplicationInitializer(Action initializer)
            : this(_ => { initializer(); return Task.CompletedTask; }) { }

        public Task InitializeAsync(CancellationToken cancellationToken) => _initializer(cancellationToken);
    }
}
