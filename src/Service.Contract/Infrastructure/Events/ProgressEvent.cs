using System.Runtime.Serialization;

namespace WebApp.Service
{
    [DataContract]
    public record class ProgressEvent : Event
    {
        [DataMember(Order = 1)] public float Progress { get; init; }
        [DataMember(Order = 2)] public string? StatusText { get; init; }
    }
}
