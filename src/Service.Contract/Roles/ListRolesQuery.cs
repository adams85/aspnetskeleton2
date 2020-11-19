using System.Runtime.Serialization;

namespace WebApp.Service.Roles
{
    [DataContract]
    public class ListRolesQuery : ListQuery<ListResult<RoleData>>
    {
        [DataMember(Order = 1)] public string? UserName { get; set; }
    }
}
