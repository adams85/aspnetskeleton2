using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public enum AuthenticateUserStatus
    {
        [EnumMember] NotExists,
        [EnumMember] Unapproved,
        [EnumMember] LockedOut,
        [EnumMember] Failed,
        [EnumMember] Successful,
    }
}
