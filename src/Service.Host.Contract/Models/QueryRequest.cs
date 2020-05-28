using System.Runtime.Serialization;

namespace WebApp.Service.Host.Models
{
    [DataContract]
    public class QueryRequest
    {
        [DataMember(Order = 1)] public string QueryTypeName { get; set; } = null!;
        [DataMember(Order = 2)] public byte[]? SerializedQuery { get; set; }
    }
}
