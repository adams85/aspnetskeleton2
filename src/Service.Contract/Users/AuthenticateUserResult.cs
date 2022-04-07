using System.Runtime.Serialization;

namespace WebApp.Service.Users;

[DataContract]
public record class AuthenticateUserResult
{
    [DataMember(Order = 1)] public int? UserId { get; init; }
    [DataMember(Order = 2)] public AuthenticateUserStatus Status { get; init; }
}
