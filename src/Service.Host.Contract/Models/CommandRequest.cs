using System.Runtime.Serialization;

namespace WebApp.Service.Host.Models
{
    [DataContract]
    public class CommandRequest
    {
        [DataMember(Order = 1)] public string CommandTypeName { get; set; } = null!;
        [DataMember(Order = 2)] public byte[]? SerializedCommand { get; set; }
    }
}
