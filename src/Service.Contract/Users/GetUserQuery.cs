using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public record class GetUserQuery : IQuery<UserData?>
    {
        [Required]
        [DataMember(Order = 1)] public UserIdentifier Identifier { get; init; } = null!;
    }
}
