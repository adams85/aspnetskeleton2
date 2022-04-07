using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public record class VerifyJwtRefreshTokenQuery : IQuery<bool>
    {
        [Required]
        [DataMember(Order = 1)] public string UserName { get; init; } = null!;

        [DataMember(Order = 2)] public TimeSpan TokenExpirationTimeSpan { get; init; }

        [Required]
        [DataMember(Order = 3)] public string VerificationToken { get; init; } = null!;
    }
}
