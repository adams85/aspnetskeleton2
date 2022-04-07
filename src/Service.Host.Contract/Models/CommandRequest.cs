using System.Runtime.Serialization;

namespace WebApp.Service.Host.Models;

[DataContract]
public record class CommandRequest
{
    [DataMember(Order = 1)] public string CommandTypeName { get; init; } = null!;
    [DataMember(Order = 2)] public byte[]? SerializedCommand { get; init; }
}
