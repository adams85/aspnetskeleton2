using System.Runtime.Serialization;

namespace WebApp.Service
{
    [DataContract]
    public class ListResult<T>
    {
        [DataMember(Order = 1)] public T[]? Items { get; set; } = null!;
        [DataMember(Order = 2)] public int TotalItemCount { get; set; }
        [DataMember(Order = 3)] public int PageIndex { get; set; }
        [DataMember(Order = 4)] public int PageSize { get; set; }
    }
}
