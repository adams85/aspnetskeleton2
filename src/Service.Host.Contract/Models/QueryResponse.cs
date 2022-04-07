using System.Runtime.Serialization;
using ProtoBuf;

namespace WebApp.Service.Host.Models
{
    [DataContract]
    [ProtoInclude(1, typeof(Success))]
    [ProtoInclude(2, typeof(Failure))]
    [ProtoInclude(3, typeof(Notification))]
    public abstract record class QueryResponse
    {
        [DataContract]
        public record class Success : QueryResponse
        {
            [DataMember(Order = 1)] public bool IsResultNull { get; init; }
            [DataMember(Order = 2)] public byte[]? SerializedResult { get; init; }
        }

        [DataContract]
        public record class Failure : QueryResponse
        {
            [DataMember(Order = 1)] public ServiceErrorData Error { get; init; } = null!;
        }

        [DataContract]
        public record class Notification : QueryResponse
        {
            [DataMember(Order = 1)] public EventData Event { get; init; } = null!;
        }
    }
}
