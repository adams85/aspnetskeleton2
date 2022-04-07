using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WebApp.Service.Users;

namespace WebApp.Service.Roles;

[DataContract]
public record class GetRoleQuery : IQuery<RoleData?>
{
    [Required]
    [DataMember(Order = 1)] public RoleIdentifier Identifier { get; init; } = null!;
}
