using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Roles
{
    [DataContract]
    public record class IsUserInRoleQuery : IQuery<bool>
    {
        [Required]
        [DataMember(Order = 1)] public string UserName { get; init; } = null!;

        [Required]
        [DataMember(Order = 2)] public string RoleName { get; init; } = null!;
    }
}
