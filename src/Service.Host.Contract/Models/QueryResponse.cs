using System.Runtime.Serialization;
using ProtoBuf;

namespace WebApp.Service.Host.Models
{
    [DataContract]
    [ProtoInclude(1, typeof(Success))]
    [ProtoInclude(2, typeof(Failure))]
    [ProtoInclude(3, typeof(Progress))]
    public abstract class QueryResponse
    {
        [DataContract]
        public class Success : QueryResponse
        {
            [DataMember(Order = 1)] public bool IsResultNull { get; set; }
            [DataMember(Order = 2)] public byte[]? SerializedResult { get; set; }
        }

        [DataContract]
        public class Failure : QueryResponse
        {
            [DataMember(Order = 1)] public ServiceErrorData Error { get; set; } = null!;
        }

        [DataContract]
        public class Progress : QueryResponse
        {
            [DataMember(Order = 1)] public ProgressEventData Event { get; set; } = null!;
        }
    }
}
