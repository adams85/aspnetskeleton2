using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WebApp.Core.Infrastructure;
using WebApp.DataAccess;
using WebApp.Service.Helpers;
using WebApp.Service.Infrastructure.Events;

namespace WebApp.Service.Settings
{
    internal sealed class SettingsSource : ISettingsSource, IDisposable
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IEventNotifier _eventNotifier;
        private readonly IClock _clock;
        private readonly ILogger _logger;

        private readonly TimeSpan _delayOnLoadError;

        private readonly TaskCompletionSource<object?> _initializedTcs;
        private readonly IDisposable _notifySubscription;

        private Action? _onInvalidate;
        private volatile SettingsChangedEvent? _lastEvent;
        private Exception? _previousLoadException;

        public SettingsSource(IServiceScopeFactory serviceScopeFactory, IEventNotifier eventNotifier, IClock clock,
            IOptions<SettingsSourceOptions>? options, ILogger<SettingsSource>? logger)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _eventNotifier = eventNotifier ?? throw new ArgumentNullException(nameof(eventNotifier));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _logger = logger ?? (ILogger)NullLogger.Instance;

            var optionsValue = options?.Value;
            _delayOnLoadError = optionsValue?.DelayOnLoadError ?? SettingsSourceOptions.DefaultDelayOnLoadError;

            _initializedTcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            var initialize = Observable
                .FromAsync(LoadAsync)
                .Do(OnLoadSuccess, OnLoadFailure)
                .Do(Noop<SettingsChangedEvent>.Action, ex => _initializedTcs.TrySetException(ex), () => _initializedTcs.TrySetResult(null));

            _notifySubscription = initialize
                .Concat(Observable
                    .FromEvent(handler => _onInvalidate += handler, handler => _onInvalidate -= handler)
                    .Select(_ => Observable.FromAsync(LoadAsync)
                       .Do(OnLoadSuccess, OnLoadFailure)
                       .Retry(wrapSubsequent: source => source.DelaySubscription(_delayOnLoadError))
                       .DoOnSubscribe(ClearLoadException))
                    .Switch())
                .Subscribe(@event =>
                {
                    _lastEvent = @event;
                    _eventNotifier.Notify(@event);
                });
        }

        public void Dispose()
        {
            _notifySubscription.Dispose();
        }

        private void ClearLoadException() => _previousLoadException = null;

        private void OnLoadSuccess(SettingsChangedEvent _) => ClearLoadException();

        private void OnLoadFailure(Exception ex)
        {
            // basic protection against littering the log with identical, recurring exceptions (e.g. connection errors, etc.)
            if (_previousLoadException?.ToString() != ex.ToString())
                _logger.LogError(ex, "Loading settings failed.");

            _previousLoadException = ex;
        }

        private Task<SettingsChangedEvent> LoadAsync(CancellationToken cancellationToken) => Task.Run(async () =>
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            using (var dbContext = scope.ServiceProvider.GetRequiredService<ReadOnlyDataContext>())
            {
                var linq = dbContext.Settings;

                return new SettingsChangedEvent
                {
                    Version = _clock.TimestampTicks,
                    Data = await linq.ToDictionaryAsync(s => s.Name, s => s.Value, cancellationToken).ConfigureAwait(false)
                };
            }
        }, cancellationToken);

        public async Task<SettingsChangedEvent> GetLatestVersionAsync(CancellationToken cancellationToken)
        {
            if (!_initializedTcs.Task.IsCompleted)
                await _initializedTcs.Task.AsCancelable(cancellationToken).ConfigureAwait(false);

            return _lastEvent!;
        }

        public void Invalidate() => _onInvalidate?.Invoke();
    }
}
