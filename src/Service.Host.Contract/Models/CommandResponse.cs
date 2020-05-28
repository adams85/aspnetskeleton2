using System.Runtime.Serialization;
using ProtoBuf;

namespace WebApp.Service.Host.Models
{
    [DataContract]
    [ProtoInclude(1, typeof(Success))]
    [ProtoInclude(2, typeof(Failure))]
    [ProtoInclude(3, typeof(Progress))]
    public abstract class CommandResponse
    {
        [DataContract]
        public class Success : CommandResponse
        {
            [DataMember(Order = 1)] public KeyData? Key { get; set; }
        }

        [DataContract]
        public class Failure : CommandResponse
        {
            [DataMember(Order = 1)] public ServiceErrorData Error { get; set; } = null!;
        }

        [DataContract]
        public class Progress : CommandResponse
        {
            [DataMember(Order = 1)] public ProgressEventData Event { get; set; } = null!;
        }
    }
}
