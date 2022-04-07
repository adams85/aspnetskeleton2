using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Roles;

[DataContract]
public record class AddUsersToRolesCommand : ICommand
{
    [Required, ItemsRequired, MinLength(1)]
    [DataMember(Order = 1)] public string[] UserNames { get; init; } = null!;

    [Required, ItemsRequired, MinLength(1)]
    [DataMember(Order = 2)] public string[] RoleNames { get; init; } = null!;
}
