using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public class GetUserQuery : IQuery<UserData?>
    {
        [Required]
        [DataMember(Order = 1)] public UserIdentifier Identifier { get; set; } = null!;
    }
}
