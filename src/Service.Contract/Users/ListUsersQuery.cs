using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public class ListUsersQuery : ListQuery<ListResult<UserData>>
    {
        [DataMember(Order = 1)] public string? UserNamePattern { get; set; }
        [DataMember(Order = 2)] public string? EmailPattern { get; set; }
        [DataMember(Order = 3)] public string? RoleName { get; set; }
    }
}
