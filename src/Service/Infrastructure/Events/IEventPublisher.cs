using System;

namespace WebApp.Service.Infrastructure.Events;

internal interface IEventPublisher
{
    void Publish(Event @event);
    IDisposable PublishMany(IObservable<Event> events);
}
