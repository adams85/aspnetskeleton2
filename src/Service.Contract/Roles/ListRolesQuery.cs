using System.Runtime.Serialization;

namespace WebApp.Service.Roles
{
    [DataContract]
    public record class ListRolesQuery : ListQuery<ListResult<RoleData>>
    {
        [DataMember(Order = 1)] public string? UserName { get; init; }
    }
}
