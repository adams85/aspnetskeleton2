using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Karambolo.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WebApp.Service.Helpers;

namespace WebApp.Service.Infrastructure.Events
{
    internal sealed class EventBus : IEventNotifier, IEventListener, IDisposable
    {
        private readonly ILogger _logger;

        private readonly Subject<Event> _subject;
        private readonly IObservable<Event> _events;

        public EventBus(ILogger<EventBus>? logger)
        {
            _logger = logger ?? (ILogger)NullLogger.Instance;

            _subject = new Subject<Event>();

            // when subject completes, IsActive should complete as well
            IsActive = _subject.Where(_ => false).Cast<bool>().StartWith(true);

            const string subscriberErrorFormat = "Error occurred in a listener's {0} handler.";

            // Subject<T> invokes the subscribed observers one after another without exception handling
            // (see https://github.com/dotnet/reactive/blob/rxnet-v4.4.1/Rx.NET/Source/src/System.Reactive/Subjects/Subject.cs#L148),
            // so we need to wrap our subject to make sure that a synchronous exception thrown in a listener's OnNext handler doesn't prevent the notification of others
            _events = Observable
                .Create<Event>(observer =>
                {
                    var safeObserver = Observer.Create<Event>(
                        value =>
                        {
                            try { observer.OnNext(value); }
                            catch (Exception ex) { _logger.LogError(ex, string.Format(subscriberErrorFormat, nameof(IObserver<object>.OnNext))); }
                        },
                        exception =>
                        {
                            try { observer.OnError(exception); }
                            catch (Exception ex) { _logger.LogError(ex, string.Format(subscriberErrorFormat, nameof(IObserver<object>.OnError))); }
                        },
                        () =>
                        {
                            try { observer.OnCompleted(); }
                            catch (Exception ex) { _logger.LogError(ex, string.Format(subscriberErrorFormat, nameof(IObserver<object>.OnCompleted))); }
                        });

                    return _subject.SubscribeSafe(safeObserver);
                });
        }

        public void Dispose()
        {
            if (!_subject.IsDisposed)
            {
                _subject.OnCompleted();
                _subject.Dispose();
            }
        }

        public IObservable<bool> IsActive { get; }

        public void Notify(Event @event) => _subject.OnNext(@event);

        public IDisposable NotifyMany(IObservable<Event> events) =>
            events
                .Do(Noop<Event>.Action, ex => _logger.LogError(ex, "A source sequence terminated with error."))
                .OnErrorResumeNext(Observable.Never<Event>())
                .Multicast(_subject)
                .Connect();

        public IObservable<Event> Listen() => _events;
    }
}
