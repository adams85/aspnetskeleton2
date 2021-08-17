using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Karambolo.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WebApp.Service.Helpers;

namespace WebApp.Service.Infrastructure.Events
{
    internal sealed class ServiceHostEventListener : IEventListener, IDisposable
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ILogger _logger;

        private readonly TimeSpan _delayOnDropout;
        private readonly BehaviorSubject<bool> _isActiveSubject;
        private readonly IObservable<Event> _events;

        private Exception? _previousException;

        public ServiceHostEventListener(IQueryDispatcher queryDispatcher, IOptions<ServiceHostEventListenerOptions>? options, ILogger<ServiceHostEventListener>? logger)
        {
            _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
            _logger = logger ?? (ILogger)NullLogger.Instance;

            var optionsValue = options?.Value;
            _delayOnDropout = optionsValue?.DelayOnDropout ?? ServiceHostEventListenerOptions.DefaultDelayOnDropout;

            _isActiveSubject = new BehaviorSubject<bool>(true);

            IsActive = _isActiveSubject
                .StartWith(true)
                .DistinctUntilChanged();

            var events = Observable
                .Create<Event>(observer =>
                {
                    var query = new StreamBusEventsQuery
                    {
                        OnEvent = (_, @event) => observer.OnNext(@event)
                    };

                    return Observable
                        .FromAsync(async ct =>
                        {
                            await _queryDispatcher.DispatchAsync(query, ct).ConfigureAwait(false);
                            throw new InvalidOperationException($"{nameof(StreamBusEventsQuery)} returned unexpectedly.");
                        })
                        .Subscribe(CachedDelegates.Noop<Unit>.Action, observer.OnError);
                })
                .Do(OnEventReceived, OnStreamError)
                .Retry(wrapSubsequent: source => source.DelaySubscription(_delayOnDropout))
                // when subject completes, event stream should complete as well
                .TakeUntil(_isActiveSubject.Where(_ => false).Materialize())
                .Publish();

            _events = events.AsObservable();

            // no need to explicitly disconnect in Dispose because of TakeUntil
            events.Connect();
        }

        public void Dispose()
        {
            if (!_isActiveSubject.IsDisposed)
            {
                _isActiveSubject.OnCompleted();
                _isActiveSubject.Dispose();
            }
        }

        public IObservable<bool> IsActive { get; }

        private void OnEventReceived(Event @event)
        {
            _previousException = null;

            _isActiveSubject.OnNext(true);

            if (@event is StreamEvent.Init)
                _logger.LogInformation("Connecting to remote event stream was successful.");
        }

        private void OnStreamError(Exception ex)
        {
            _isActiveSubject.OnNext(false);

            // basic protection against littering the log with identical, recurring exceptions (e.g. connection errors, etc.)
            if (_previousException?.ToString() != ex.ToString())
                _logger.LogError(ex, "Connecting to remote event stream failed.");

            _previousException = ex;
        }

        public IObservable<Event> Listen() => _events;
    }
}
