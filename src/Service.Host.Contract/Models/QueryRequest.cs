using System.Runtime.Serialization;

namespace WebApp.Service.Host.Models;

[DataContract]
public record class QueryRequest
{
    [DataMember(Order = 1)] public string QueryTypeName { get; init; } = null!;
    [DataMember(Order = 2)] public byte[]? SerializedQuery { get; init; }
}
