using System;
using System.Collections;
using System.Runtime.Serialization;

namespace WebApp.Service
{
    [DataContract]
    public record class ListResult<T> : IListResult
    {
        [DataMember(Order = 1)] public T[]? Items { get; init; }
        [DataMember(Order = 2)] public int TotalItemCount { get; init; }
        [DataMember(Order = 3)] public int PageIndex { get; init; }
        [DataMember(Order = 4)] public int PageSize { get; init; }

        IList? IListResult.Items => Items;
    }
}
