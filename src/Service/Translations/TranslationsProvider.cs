using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using Karambolo.PO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WebApp.Service.Helpers;
using WebApp.Service.Infrastructure;
using WebApp.Service.Infrastructure.Events;

namespace WebApp.Service.Translations
{
    internal sealed class TranslationsProvider : ITranslationsProvider, IDisposable
    {
        private static readonly IEqualityComparer<(string, string)> s_translationsKeyComparer = DelegatedEqualityComparer.Create<(string Location, string Culture)>(
            comparer: (x, y) => StringComparer.OrdinalIgnoreCase.Equals(x.Location, y.Location) && StringComparer.OrdinalIgnoreCase.Equals(x.Culture, y.Culture),
            hasher: obj =>
            {
                var hashCode = new HashCode();
                hashCode.Add(obj.Location, StringComparer.OrdinalIgnoreCase);
                hashCode.Add(obj.Culture, StringComparer.OrdinalIgnoreCase);
                return hashCode.ToHashCode();
            });

        private readonly IEventListener _eventListener;
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ILogger _logger;

        private readonly TimeSpan _delayOnRefreshError;

        private readonly TaskCompletionSource<object?> _initializedTcs;
        private readonly IDisposable _refreshSubscription;

        private bool _resetting;
        private readonly Dictionary<(string Location, string Culture), (TranslationsChangedEvent LastEvent, POCatalog? Catalog)> _lastEvents;
        private volatile IReadOnlyDictionary<(string Location, string Culture), POCatalog>? _catalogs;

        private Exception? _previousResetException;

        public TranslationsProvider(IEventListener eventListener, IQueryDispatcher queryDispatcher, IOptions<TranslationsProviderOptions>? options, ILogger<TranslationsProvider>? logger)
        {
            _eventListener = eventListener ?? throw new ArgumentNullException(nameof(eventListener));
            _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
            _logger = logger ?? (ILogger)NullLogger.Instance;

            var optionsValue = options?.Value;
            _delayOnRefreshError = optionsValue?.DelayOnRefreshError ?? TranslationsProviderOptions.DefaultDelayOnRefreshError;

            _lastEvents = new Dictionary<(string, string), (TranslationsChangedEvent, POCatalog?)>(s_translationsKeyComparer);

            _initializedTcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            // event listener doesn't provide guaranteed delivery (currently), so we might miss some change notifications during a dropout;
            // thus, we should also refresh the internal cache after connection has been restored
            _refreshSubscription = _eventListener.IsActive
                .Where(isActive => isActive)
                .Select(_ => _eventListener.Listen<TranslationsChangedEvent>()
                    .Select(@event => ((TranslationsChangedEvent[]?)null, @event))!
                    .Merge<(TranslationsChangedEvent[], TranslationsChangedEvent?)>(Observable
                        .FromAsync(ct => _queryDispatcher.DispatchAsync(new GetLatestTranslationsQuery { }, ct))
                        .Do(OnResetSuccess, OnResetError)
                        .Retry(wrapSubsequent: source => source.DelaySubscription(_delayOnRefreshError))
                        .Select(@events => (@events ?? Array.Empty<TranslationsChangedEvent>(), (TranslationsChangedEvent?)null))
                        .DoOnSubscribe(ClearResetException))!
                    .StartWith(((TranslationsChangedEvent[]?)null, (TranslationsChangedEvent?)null)))
                .Switch()
                .Subscribe(item =>
                {
                    var (initialEvents, @event) = item;
                    try
                    {
                        if (Refresh(initialEvents, @event))
                        {
                            if (initialEvents != null)
                                _initializedTcs.TrySetResult(null);

                            _logger.LogInformation("Internal cache was refreshed.");
                        }
                    }
                    catch (Exception ex) when (initialEvents != null)
                    {
                        _initializedTcs.TrySetException(ex);
                    }
                });
        }

        private void ClearResetException() => Volatile.Write(ref _previousResetException, null);

        private void OnResetSuccess(TranslationsChangedEvent[]? _) => ClearResetException();

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

        private bool RefreshCore(TranslationsChangedEvent @event)
        {
            var key = (@event.Location, @event.Culture);

            if (_lastEvents.TryGetValue(key, out var value))
            {
                var (lastEvent, _) = value;
                if (lastEvent.Version >= @event.Version || lastEvent.Location != @event.Location || lastEvent.Culture != @event.Culture)
                    return false;
            }

            _lastEvents[key] =
            (
                new TranslationsChangedEvent
                {
                    Version = @event.Version,
                    Location = @event.Location,
                    Culture = @event.Culture,
                },
                @event.Data?.ToCatalog()
            );

            return true;
        }

        private bool Refresh(TranslationsChangedEvent[]? initialEvents, TranslationsChangedEvent? @event)
        {
            bool hasRefreshed;

            lock (_lastEvents)
            {
                if (initialEvents != null)
                {
                    for (int i = 0; i < initialEvents.Length; i++)
                        RefreshCore(initialEvents[i]);

                    hasRefreshed = true;
                    _resetting = false;
                }
                else if (@event != null)
                {
                    hasRefreshed = RefreshCore(@event) ? !_resetting : false;
                }
                else
                {
                    _resetting = true;
                    _lastEvents.Clear();
                    hasRefreshed = false;
                }

                if (hasRefreshed)
                {
                    _catalogs = _lastEvents
                        .Where(item => item.Value.Catalog != null)
                        .ToDictionary(item => item.Key, item => item.Value.Catalog!, s_translationsKeyComparer);
                }
            }

            return hasRefreshed;
        }

        public IReadOnlyDictionary<(string, string), POCatalog> GetCatalogs()
        {
            if (!Initialization.IsCompleted)
                throw new InvalidOperationException($"Service has not been initialized yet. Await the task returned by {nameof(Initialization)} at startup to avoid this error.");

            return _catalogs!;
        }
    }
}
