using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public enum AuthenticateUserStatus
    {
        [EnumMember] Unknown,
        [EnumMember] Failed,
        [EnumMember] Unapproved,
        [EnumMember] LockedOut,
        [EnumMember] Successful,
    }
}
