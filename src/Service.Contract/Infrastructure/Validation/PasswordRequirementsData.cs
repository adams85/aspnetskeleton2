using System.Runtime.Serialization;

namespace WebApp.Service.Infrastructure.Validation
{
    [DataContract]
    public class PasswordRequirementsData
    {
        [DataMember(Order = 1)] public int RequiredLength { get; set; }
        [DataMember(Order = 2)] public int RequiredUniqueChars { get; set; }
        [DataMember(Order = 3)] public bool RequireNonAlphanumeric { get; set; }
        [DataMember(Order = 4)] public bool RequireLowercase { get; set; }
        [DataMember(Order = 5)] public bool RequireUppercase { get; set; }
        [DataMember(Order = 6)] public bool RequireDigit { get; set; }
    }
}
