using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Karambolo.Common;
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
        private readonly IEventListener _eventListener;
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ILogger _logger;

        private readonly TimeSpan _delayOnRefreshError;

        private readonly object _gate;
        private readonly TaskCompletionSource<object?> _initializedTcs;
        private readonly IDisposable _refreshSubscription;

        private volatile SettingsChangedEvent? _lastEvent;

        private Exception? _previousResetException;

        public SettingsProvider(IEventListener eventListener, IQueryDispatcher queryDispatcher, IOptions<SettingsProviderOptions>? options, ILogger<SettingsProvider>? logger)
        {
            _eventListener = eventListener ?? throw new ArgumentNullException(nameof(eventListener));
            _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
            _logger = logger ?? (ILogger)NullLogger.Instance;

            var optionsValue = options?.Value;
            _delayOnRefreshError = optionsValue?.DelayOnRefreshError ?? SettingsProviderOptions.DefaultDelayOnRefreshError;

            _gate = new object();
            _initializedTcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            // event listener doesn't provide guarenteed delivery (currently), so we might miss some change notifications during a dropout;
            // thus, we should also refresh the internal cache after connection has been restored
            _refreshSubscription = _eventListener.IsActive
                .Where(isActive => isActive)
                .Select(_ => _eventListener.Listen<SettingsChangedEvent>()
                    .Merge(Observable
                        .FromAsync(ct => _queryDispatcher.DispatchAsync(new GetLatestSettingsQuery { }, ct))
                        .Do(OnResetSuccess, OnResetFailure)
                        .Retry(wrapSubsequent: source => source.DelaySubscription(_delayOnRefreshError))
                        .DoOnSubscribe(ClearResetException)))
                .Switch()
                .Subscribe(@event =>
                {
                    lock (_gate)
                        if (_lastEvent == null || _lastEvent!.Version < @event.Version)
                        {
                            @event.Data ??= new Dictionary<string, string?>();
                            _lastEvent = @event;
                        }

                    _initializedTcs.TrySetResult(null);

                    _logger.LogInformation("Internal cache was refreshed.");
                });
        }

        private void ClearResetException() => _previousResetException = null;

        private void OnResetSuccess(SettingsChangedEvent _) => ClearResetException();

        private void OnResetFailure(Exception ex)
        {
            // basic protection against littering the log with identical, recurring exceptions (e.g. connection errors, etc.)
            if (_previousResetException?.ToString() != ex.ToString())
                _logger.LogError(ex, "Resetting internal cache failed.");

            _previousResetException = ex;
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

            return _lastEvent!.Data!;
        }
    }
}
