using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public record class AuthenticateUserQuery : IQuery<AuthenticateUserResult>
    {
        [Required]
        [DataMember(Order = 1)] public string UserName { get; init; } = null!;

        [Required]
        [DataMember(Order = 2)] public string Password { get; init; } = null!;
    }
}
