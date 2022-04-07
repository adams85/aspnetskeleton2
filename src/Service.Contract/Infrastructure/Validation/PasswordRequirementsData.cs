using System.Runtime.Serialization;

namespace WebApp.Service.Infrastructure.Validation;

[DataContract]
public record class PasswordRequirementsData
{
    [DataMember(Order = 1)] public int RequiredLength { get; init; }
    [DataMember(Order = 2)] public int RequiredUniqueChars { get; init; }
    [DataMember(Order = 3)] public bool RequireNonAlphanumeric { get; init; }
    [DataMember(Order = 4)] public bool RequireLowercase { get; init; }
    [DataMember(Order = 5)] public bool RequireUppercase { get; init; }
    [DataMember(Order = 6)] public bool RequireDigit { get; init; }
}
