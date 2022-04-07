using System.Runtime.Serialization;

namespace WebApp.Service.Roles;

[DataContract]
public record class RoleData
{
    [DataMember(Order = 1)] public int RoleId { get; init; }
    [DataMember(Order = 2)] public string RoleName { get; init; } = null!;
    [DataMember(Order = 3)] public string? Description { get; init; } = null!;
}
