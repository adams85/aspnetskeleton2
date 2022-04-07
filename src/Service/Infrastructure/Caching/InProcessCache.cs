using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace WebApp.Service.Infrastructure.Caching;

internal sealed class InProcessCache : ICache
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, ScopeToken> _scopeTokens = new ConcurrentDictionary<string, ScopeToken>();

    public InProcessCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    private ScopeToken GetOrCreateScopeToken(string scopeKey)
    {
        return _scopeTokens.GetOrAdd(scopeKey, k => new ScopeToken(this, k));
    }

    private void RemoveScopeToken(string scopeKey)
    {
        _scopeTokens.TryRemove(scopeKey, out _);
    }

    public Task<T> GetOrAddAsync<T>(string key, Func<string, CancellationToken, Task<T>> valueFactoryAsync, CacheOptions options,
        IEnumerable<string> scopes, CancellationToken cancellationToken)
    {
        return _memoryCache.GetOrCreateAsync(key, ce =>
        {
            if (options != null)
            {
                ce.AbsoluteExpirationRelativeToNow = options.AbsoluteExpiration;
                ce.SlidingExpiration = options.SlidingExpiration;
                ce.Priority = options.Priority ?? CacheOptions.DefaultPriority;
            }

            if (scopes != null)
                foreach (var token in scopes.Select(GetOrCreateScopeToken))
                    ce.ExpirationTokens.Add(token);

            return valueFactoryAsync((string)ce.Key, cancellationToken);
        });
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        _memoryCache.Remove(key);

        return Task.CompletedTask;
    }

    public Task RemoveScopeAsync(string scope, CancellationToken cancellationToken)
    {
        if (_scopeTokens.TryGetValue(scope, out var token))
            token.NotifyChanged();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _memoryCache?.Dispose();
    }

    private sealed class ScopeTokenRegistration : IDisposable
    {
        private readonly ScopeToken _owner;
        private readonly Action<object> _callback;
        private readonly object _state;

        public ScopeTokenRegistration(ScopeToken owner, Action<object> callback, object state)
        {
            _owner = owner;
            _callback = callback;
            _state = state;
        }

        public void InvokeCallback()
        {
            _callback(_state);
        }

        public void Dispose()
        {
            _owner.UnregisterChangeCallback(this);
        }
    }

    private sealed class ScopeToken : IChangeToken
    {
        private readonly InProcessCache _owner;
        private readonly string _key;
        private readonly List<ScopeTokenRegistration> _registrations = new List<ScopeTokenRegistration>();
        private bool _hasChanged;

        public ScopeToken(InProcessCache owner, string key)
        {
            _owner = owner;
            _key = key;
        }

        public void NotifyChanged()
        {
            ScopeTokenRegistration[] registrations;
            lock (_registrations)
            {
                _hasChanged = true;
                registrations = _registrations.ToArray();
            }

            for (int i = 0, n = registrations.Length; i < n; i++)
                registrations[i].InvokeCallback();
        }

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            lock (_registrations)
                if (!_hasChanged)
                {
                    var registration = new ScopeTokenRegistration(this, callback, state);
                    _registrations.Add(registration);
                    return registration;
                }
                else
                    return Disposable.Empty;
        }

        public void UnregisterChangeCallback(ScopeTokenRegistration registration)
        {
            lock (_registrations)
                if (_registrations.Remove(registration) && _registrations.Count == 0)
                    _owner.RemoveScopeToken(_key);
        }

        public bool HasChanged => _hasChanged;

        public bool ActiveChangeCallbacks => true;
    }
}
