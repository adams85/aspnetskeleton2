using System;
using System.Reactive.Linq;

namespace WebApp.Service.Infrastructure.Events
{
    internal static class EventListenerExtensions
    {
        public static IObservable<TEvent> Listen<TEvent>(this IEventListener listener) where TEvent : Event =>
            listener.Listen().OfType<TEvent>();
    }
}
