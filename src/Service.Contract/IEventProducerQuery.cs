using System;

namespace WebApp.Service;

public interface IEventProducerQuery
{
    Action<IQuery, Event>? OnEvent { get; set; }
}
