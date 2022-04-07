using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public record class CachedUserInfoData
    {
        [DataMember(Order = 1)] public int UserId { get; init; }
        [DataMember(Order = 2)] public string UserName { get; init; } = null!;
        [DataMember(Order = 3)] public string Email { get; init; } = null!;
        [DataMember(Order = 4)] public string? FirstName { get; init; }
        [DataMember(Order = 5)] public string? LastName { get; init; }
        [DataMember(Order = 6)] public bool LoginAllowed { get; init; }
        [DataMember(Order = 7)] public string[]? Roles { get; set; }
    }
}
