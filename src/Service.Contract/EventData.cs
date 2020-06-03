using System.Runtime.Serialization;

namespace WebApp.Service
{
    [DataContract]
    public sealed class EventData
    {
        [DataMember(Order = 1)] public Event Value { get; set; } = default!;
    }
}
