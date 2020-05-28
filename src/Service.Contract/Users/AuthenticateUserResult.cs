using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public class AuthenticateUserResult
    {
        [DataMember(Order = 1)] public int? UserId { get; set; }
        [DataMember(Order = 2)] public AuthenticateUserStatus Status { get; set; }
    }
}
