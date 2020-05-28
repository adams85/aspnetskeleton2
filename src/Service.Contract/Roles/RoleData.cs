using System.Runtime.Serialization;

namespace WebApp.Service.Roles
{
    [DataContract]
    public class RoleData
    {
        [DataMember(Order = 1)] public int RoleId { get; set; }
        [DataMember(Order = 2)] public string RoleName { get; set; } = null!;
        [DataMember(Order = 3)] public string? Description { get; set; } = null!;
    }
}
