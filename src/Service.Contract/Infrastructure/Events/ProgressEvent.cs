using System.Runtime.Serialization;

namespace WebApp.Service
{
    [DataContract]
    public class ProgressEvent : Event
    {
        [DataMember(Order = 1)] public float? Progress { get; set; }
        [DataMember(Order = 2)] public string? StatusText { get; set; }
    }
}
