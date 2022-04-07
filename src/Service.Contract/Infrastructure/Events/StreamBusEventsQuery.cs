using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Infrastructure.Events;

[DataContract]
public record class StreamBusEventsQuery : IQuery<long>, IEventProducerQuery
{
    [Required]
    public Action<IQuery, Event>? OnEvent { get; set; }
}
