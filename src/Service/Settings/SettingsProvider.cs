using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WebApp.Service.Helpers;
using WebApp.Service.Infrastructure;
using WebApp.Service.Infrastructure.Events;

namespace WebApp.Service.Settings
{
    internal sealed class SettingsProvider : ISettingsProvider, IDisposable
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ILogger _logger;

        private readonly TimeSpan _delayOnRefreshError;

        private readonly TaskCompletionSource<object?> _initializedTcs;
        private readonly IDisposable _refreshSubscription;

        private volatile IReadOnlyDictionary<string, string?>? _settings;

        private Exception? _previousException;

        public SettingsProvider(IEventListener eventListener, IQueryDispatcher queryDispatcher, IOptions<SettingsProviderOptions>? options, ILogger<SettingsProvider>? logger)
        {
            if (eventListener == null)
                throw new ArgumentNullException(nameof(eventListener));

            _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
            _logger = logger ?? (ILogger)NullLogger.Instance;

            var optionsValue = options?.Value;
            _delayOnRefreshError = optionsValue?.DelayOnRefreshError ?? SettingsProviderOptions.DefaultDelayOnRefreshError;

            _initializedTcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            _refreshSubscription = eventListener.Listen<SettingsChangedEvent>()
                // event listener doesn't provide guarenteed delivery (currently), so we might miss some change notifications during a dropout;
                // thus, we should also refresh the internal cache after connection has been restored
                .Merge(eventListener.IsActive.Where(isActive => isActive).Select(_ => default(SettingsChangedEvent)!))
                .Select(_ => Observable.FromAsync(ct => _queryDispatcher.DispatchAsync(new GetCachedSettingsQuery { }, ct))
                    .Do(OnRefreshSuccess, OnRefreshFailure)
                    .RetryAfterDelay(_delayOnRefreshError))
                .Switch()
                .Subscribe(settings =>
                {
                    _settings = settings;
                    _initializedTcs.TrySetResult(null!);
                });
        }

        private void OnRefreshSuccess(object _)
        {
            _previousException = null;

            _logger.LogInformation("Refreshing internal cache was successful.");
        }

        private void OnRefreshFailure(Exception ex)
        {
            // basic protection against littering the log with identical, recurring exceptions (e.g. connection errors, etc.)
            if (_previousException?.ToString() != ex.ToString())
                _logger.LogError(ex, "Refreshing internal cache failed.");

            _previousException = ex;
        }

        public void Dispose()
        {
            _refreshSubscription.Dispose();
        }

        public Task Initialization => _initializedTcs.Task;

        public string? this[string name] => GetAllSettings().TryGetValue(name, out var value) ? value : null;

        public IReadOnlyDictionary<string, string?> GetAllSettings()
        {
            if (!Initialization.IsCompleted)
                throw new InvalidOperationException($"Service has not been initialized yet. Await the task returned by {nameof(Initialization)} at startup to avoid this error.");

            return _settings!;
        }
    }
}
