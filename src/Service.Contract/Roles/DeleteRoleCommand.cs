using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Roles
{
    [DataContract]
    public record class DeleteRoleCommand : ICommand
    {
        [Required]
        [DataMember(Order = 1)] public string RoleName { get; init; } = null!;

        [DataMember(Order = 2)] public bool DeletePopulatedRole { get; init; }
    }
}
