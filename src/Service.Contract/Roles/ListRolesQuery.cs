using System.Runtime.Serialization;

namespace WebApp.Service.Roles
{
    [DataContract]
    public class ListRolesQuery : ListQuery, IQuery<ListResult<RoleData>>
    {
        [DataMember(Order = 1)] public string? UserName { get; set; }
    }
}
