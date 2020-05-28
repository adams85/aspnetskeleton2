using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public class GetCachedUserInfoQuery : IQuery<CachedUserInfoData?>
    {
        [Required]
        [DataMember(Order = 1)] public string UserName { get; set; } = null!;
    }
}
