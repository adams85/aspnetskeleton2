using System.Runtime.Serialization;

namespace WebApp.Service.Users;

[DataContract]
public record class ListUsersQuery : ListQuery<ListResult<UserData>>
{
    [DataMember(Order = 1)] public string? UserNamePattern { get; init; }
    [DataMember(Order = 2)] public string? EmailPattern { get; init; }
    [DataMember(Order = 3)] public string? RoleName { get; init; }
}
