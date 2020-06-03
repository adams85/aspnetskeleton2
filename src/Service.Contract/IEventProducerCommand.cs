using System;

namespace WebApp.Service
{
    public interface IEventProducerCommand
    {
        Action<ICommand, Event>? OnEvent { get; set; }
    }
}
