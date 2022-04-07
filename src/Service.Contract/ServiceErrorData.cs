using System.Runtime.Serialization;

namespace WebApp.Service;

[DataContract]
public record class ServiceErrorData
{
    [DataMember(Order = 1)] public ServiceErrorCode ErrorCode { get; init; }
    [DataMember(Order = 2)] public ServiceErrorArgData[]? Args { get; init; }
}
