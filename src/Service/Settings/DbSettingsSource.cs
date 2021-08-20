using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WebApp.Core.Helpers;
using WebApp.Core.Infrastructure;
using WebApp.DataAccess;
using WebApp.Service.Infrastructure.Events;

namespace WebApp.Service.Settings
{
    internal sealed class DbSettingsSource : ISettingsSource, IDisposable
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IEventPublisher _eventPublisher;
        private readonly IClock _clock;
        private readonly ILogger _logger;

        private readonly object _gate;
        private readonly TaskCompletionSource<object?> _initializedTcs;
        private readonly IDisposable _notifySubscription;

        private Action? _onInvalidate;
        private volatile SettingsChangedEvent? _lastEvent;

        public DbSettingsSource(IServiceScopeFactory serviceScopeFactory, IEventPublisher eventPublisher, IClock clock,
            ILogger<DbSettingsSource>? logger)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _logger = logger ?? (ILogger)NullLogger.Instance;

            _gate = new object();
            _initializedTcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            var initialize = Load()
                .Do(Noop<SettingsChangedEvent>.Action, ex => _initializedTcs.TrySetException(ex), () => _initializedTcs.TrySetResult(null));

            _notifySubscription = initialize
                .Concat(Observable
                    .FromEvent(handler => _onInvalidate += handler, handler => _onInvalidate -= handler)
                    .Select(_ => Load()
                        .Catch<SettingsChangedEvent, Exception>(ex => Observable.Empty<SettingsChangedEvent>()))
                    .Switch())
                .Subscribe(@event =>
                {
                    if (RegisterChange(@event))
                    {
                        _logger.LogInformation("Settings have been loaded.");
                        _eventPublisher.Publish(@event);
                    }
                });
        }

        public void Dispose()
        {
            _notifySubscription.Dispose();
        }

        private Task<SettingsChangedEvent> LoadAsync(CancellationToken cancellationToken) => Task.Run(async () =>
        {
            await using (DisposableAdapter.From(_serviceScopeFactory.CreateScope(), out var scope).ConfigureAwait(false))
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ReadOnlyDataContext>();

                var linq = dbContext.Settings;

                return new SettingsChangedEvent
                {
                    Version = _clock.TimestampTicks,
                    Data = await linq.ToDictionaryAsync(s => s.Name, s => s.Value, cancellationToken).ConfigureAwait(false)
                };
            }
        }, cancellationToken);

        private IObservable<SettingsChangedEvent> Load() => Observable
            .FromAsync(LoadAsync)
            .Do(Noop<SettingsChangedEvent>.Action, ex => _logger.LogError(ex, "Unexpected error occurred when loading settings."));

        private bool RegisterChange(SettingsChangedEvent @event)
        {
            lock (_gate)
            {
                if (_lastEvent != null && _lastEvent.Version >= @event.Version)
                    return false;

                _lastEvent = @event;
                return true;
            }
        }

        public async Task<SettingsChangedEvent> GetLatestVersionAsync(CancellationToken cancellationToken)
        {
            if (!_initializedTcs.Task.IsCompleted)
                await _initializedTcs.Task.AsCancelable(cancellationToken).ConfigureAwait(false);

            return _lastEvent!;
        }

        public void Invalidate() => _onInvalidate?.Invoke();
    }
}
