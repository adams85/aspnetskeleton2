using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public class CachedUserInfoData
    {
        [DataMember(Order = 1)] public int UserId { get; set; }
        [DataMember(Order = 2)] public string UserName { get; set; } = null!;
        [DataMember(Order = 3)] public string Email { get; set; } = null!;
        [DataMember(Order = 4)] public string? FirstName { get; set; }
        [DataMember(Order = 5)] public string? LastName { get; set; }
        [DataMember(Order = 6)] public bool LoginAllowed { get; set; }
        [DataMember(Order = 7)] public string[]? Roles { get; set; }
    }
}
