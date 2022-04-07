using System.Runtime.Serialization;
using ProtoBuf;

namespace WebApp.Service.Host.Models;

[DataContract]
[ProtoInclude(1, typeof(Success))]
[ProtoInclude(2, typeof(Failure))]
[ProtoInclude(3, typeof(Notification))]
public abstract record class CommandResponse
{
    [DataContract]
    public record class Success : CommandResponse
    {
        [DataMember(Order = 1)] public KeyData? Key { get; init; }
    }

    [DataContract]
    public record class Failure : CommandResponse
    {
        [DataMember(Order = 1)] public ServiceErrorData Error { get; init; } = null!;
    }

    [DataContract]
    public record class Notification : CommandResponse
    {
        [DataMember(Order = 1)] public EventData Event { get; init; } = null!;
    }
}
