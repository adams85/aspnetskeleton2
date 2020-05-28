using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Roles
{
    [DataContract]
    public class AddUsersToRolesCommand : ICommand
    {
        [Required, ItemsRequired, MinLength(1)]
        [DataMember(Order = 1)] public string[] UserNames { get; set; } = null!;

        [Required, ItemsRequired, MinLength(1)]
        [DataMember(Order = 2)] public string[] RoleNames { get; set; } = null!;
    }
}
