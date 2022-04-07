using System;
using System.Runtime.Serialization;

namespace WebApp.Service.Users;

[DataContract]
public record class GetOnlineUserCountQuery : IQuery<int>
{
    [DataMember(Order = 1)] public DateTime DateFrom { get; init; }
}
