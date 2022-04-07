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

namespace WebApp.Service.Settings;

internal sealed class SettingsProvider : ISettingsProvider, IDisposable
{
    private readonly IEventListener _eventListener;
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly ILogger _logger;

    private readonly TimeSpan _delayOnRefreshError;

    private readonly object _gate;
    private readonly TaskCompletionSource _initializedTcs;
    private readonly IDisposable _refreshSubscription;

    private bool _resetting;
    private SettingsChangedEvent? _lastEvent;
    private volatile IReadOnlyDictionary<string, string?>? _settings;

    private Exception? _previousResetException;

    public SettingsProvider(IEventListener eventListener, IQueryDispatcher queryDispatcher, IOptions<SettingsProviderOptions>? options, ILogger<SettingsProvider>? logger)
    {
        _eventListener = eventListener ?? throw new ArgumentNullException(nameof(eventListener));
        _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
        _logger = logger ?? (ILogger)NullLogger.Instance;

        var optionsValue = options?.Value;
        _delayOnRefreshError = optionsValue?.DelayOnRefreshError ?? SettingsProviderOptions.DefaultDelayOnRefreshError;

        _gate = new object();
        _initializedTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // event listener doesn't provide guaranteed delivery (currently), so we might miss some change notifications during a dropout;
        // thus, we should also refresh the internal cache after connection has been restored
        _refreshSubscription = _eventListener.IsActive
            .Where(isActive => isActive)
            .Select(_ => _eventListener.Listen<SettingsChangedEvent>()
                .Select(@event => (false, @event))
                .Merge(Observable
                    .FromAsync(ct => _queryDispatcher.DispatchAsync(new GetLatestSettingsQuery { }, ct))
                    .Do(OnResetSuccess, OnResetError)
                    .Retry(wrapSubsequent: source => source.DelaySubscription(_delayOnRefreshError))
                    .Select(@event => (true, @event))
                    .DoOnSubscribe(ClearResetException))!
                .StartWith((true, null)))
            .Switch()
            .Subscribe(item =>
            {
                var (isInitial, @event) = item;
                try
                {
                    if (Refresh(isInitial, @event))
                    {
                        if (isInitial)
                            _initializedTcs.TrySetResult();

                        _logger.LogInformation("Internal cache was refreshed.");
                    }
                }
                catch (Exception ex) when (isInitial)
                {
                    _initializedTcs.TrySetException(ex);
                }
            });
    }

    private void ClearResetException() => Volatile.Write(ref _previousResetException, null);

    private void OnResetSuccess(SettingsChangedEvent _) => ClearResetException();

    private void OnResetError(Exception ex)
    {
        // basic protection against littering the log with identical, recurring exceptions (e.g. connection errors, etc.)
        var previousException = Interlocked.Exchange(ref _previousResetException, ex);
        if (previousException?.ToString() != ex.ToString())
            _logger.LogError(ex, "Resetting internal cache failed.");
    }

    public void Dispose()
    {
        _refreshSubscription.Dispose();
    }

    public Task Initialization => _initializedTcs.Task;

    private bool RefreshCore(SettingsChangedEvent @event)
    {
        if (_lastEvent != null && _lastEvent.Version >= @event.Version)
            return false;

        _lastEvent = new SettingsChangedEvent
        {
            Version = @event.Version,
            Data = @event.Data
        };

        return true;
    }

    private bool Refresh(bool isInitial, SettingsChangedEvent? @event)
    {
        bool hasRefreshed;

        lock (_gate)
        {
            if (@event == null)
            {
                _resetting = true;
                _lastEvent = null;
                hasRefreshed = false;
            }
            else if (isInitial)
            {
                RefreshCore(@event);
                hasRefreshed = true;
                _resetting = false;
            }
            else
            {
                hasRefreshed = RefreshCore(@event) ? !_resetting : false;
            }

            if (hasRefreshed)
                _settings = _lastEvent!.Data ?? new Dictionary<string, string?>();
        }

        return hasRefreshed;
    }

    public string? this[string name] => GetAllSettings().TryGetValue(name, out var value) ? value : null;

    public IReadOnlyDictionary<string, string?> GetAllSettings()
    {
        if (!Initialization.IsCompleted)
            throw new InvalidOperationException($"Service has not been initialized yet. Await the task returned by {nameof(Initialization)} at startup to avoid this error.");

        return _settings!;
    }
}
