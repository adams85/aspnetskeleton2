using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Service.Infrastructure.Events
{
    internal sealed class StreamBusEventsQueryHandler : QueryHandler<StreamBusEventsQuery, long>
    {
        private readonly IEventListener _eventListener;

        public StreamBusEventsQueryHandler(IEventListener eventListener)
        {
            _eventListener = eventListener ?? throw new ArgumentNullException(nameof(eventListener));
        }

        public override async Task<long> HandleAsync(StreamBusEventsQuery query, QueryContext context, CancellationToken cancellationToken)
        {
            int eventCount = 0;

            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            using (var ctr = cancellationToken.Register(() => tcs.TrySetResult()))
            using (var subscription = SubscribeToEventBus())
                await tcs.Task.ConfigureAwait(false);

            return eventCount;

            IDisposable SubscribeToEventBus() =>
                _eventListener.Listen<Event>()
                    .StartWith(new StreamEvent.Init())
                    .Subscribe(@event =>
                    {
                        eventCount++;
                        query.OnEvent!(query, @event);
                    });
        }
    }
}
