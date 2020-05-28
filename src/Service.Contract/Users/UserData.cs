using System;
using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public class UserData
    {
        [DataMember(Order = 1)] public int UserId { get; set; }
        [DataMember(Order = 2)] public string UserName { get; set; } = null!;
        [DataMember(Order = 3)] public string Email { get; set; } = null!;
        [DataMember(Order = 4)] public bool IsLockedOut { get; set; }
        [DataMember(Order = 5)] public bool IsApproved { get; set; }
        [DataMember(Order = 6)] public DateTime CreationDate { get; set; }
        [DataMember(Order = 7)] public DateTime LastPasswordChangedDate { get; set; }
        [DataMember(Order = 8)] public DateTime? LastActivityDate { get; set; }
        [DataMember(Order = 9)] public DateTime? LastLoginDate { get; set; }
        [DataMember(Order = 10)] public DateTime? LastLockoutDate { get; set; }
    }
}
