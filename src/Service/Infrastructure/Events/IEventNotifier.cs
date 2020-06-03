using System;

namespace WebApp.Service.Infrastructure.Events
{
    internal interface IEventNotifier
    {
        void Notify(Event @event);
        IDisposable NotifyMany(IObservable<Event> events);
    }
}
