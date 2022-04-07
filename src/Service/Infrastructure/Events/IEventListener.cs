using System;

namespace WebApp.Service.Infrastructure.Events;

internal interface IEventListener
{
    /// <summary>
    /// Signals when the listener losts (<c>false</c>) or restores (<c>true</c>) connection to the source of events.
    /// (Always signals <c>true</c> upon subscription.)
    /// </summary>
    /// <remarks>
    /// Events may be transmitted over an unreliable medium (e.g. over network in the case of distributed application architecture)
    /// but we don't want to implement guaranteed delivery (unless absolutely necessary) as it would be pretty complicated.
    /// Instead, we provide this sequence for the listeners so that they can take action after a potential dropout of events.
    /// </remarks>
    IObservable<bool> IsActive { get; }

    IObservable<Event> Listen();
}
