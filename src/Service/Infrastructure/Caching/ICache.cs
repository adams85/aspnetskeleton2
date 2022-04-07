using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Infrastructure.Caching;

internal interface ICache : IDisposable
{
    Task<T> GetOrAddAsync<T>(string key, Func<string, CancellationToken, Task<T>> valueFactoryAsync, CacheOptions options,
        IEnumerable<string> scopes, CancellationToken cancellationToken);
    Task RemoveAsync(string key, CancellationToken cancellationToken);
    Task RemoveScopeAsync(string scope, CancellationToken cancellationToken);
}
