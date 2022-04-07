using System;
using System.Runtime.Serialization;

namespace WebApp.Service.Users;

[DataContract]
public record class UserData
{
    [DataMember(Order = 1)] public int UserId { get; init; }
    [DataMember(Order = 2)] public string UserName { get; init; } = null!;
    [DataMember(Order = 3)] public string Email { get; init; } = null!;
    [DataMember(Order = 4)] public bool IsLockedOut { get; init; }
    [DataMember(Order = 5)] public bool IsApproved { get; init; }
    [DataMember(Order = 6)] public DateTime CreationDate { get; init; }
    [DataMember(Order = 7)] public DateTime LastPasswordChangedDate { get; init; }
    [DataMember(Order = 8)] public DateTime? LastActivityDate { get; init; }
    [DataMember(Order = 9)] public DateTime? LastLoginDate { get; init; }
    [DataMember(Order = 10)] public DateTime? LastLockoutDate { get; init; }
}
