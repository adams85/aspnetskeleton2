using System.Runtime.Serialization;

namespace WebApp.Service
{
    [DataContract]
    public sealed record class EventData
    {
        [DataMember(Order = 1)] public Event Value { get; init; } = null!;
    }
}
