using System.Runtime.Serialization;

namespace WebApp.Service
{
    [DataContract]
    public class ServiceErrorData
    {
        [DataMember(Order = 1)] public ServiceErrorCode ErrorCode { get; set; }
        [DataMember(Order = 2)] public ServiceErrorArgData[]? Args { get; set; }
    }
}
