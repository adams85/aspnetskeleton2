using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public class VerifyJwtRefreshTokenQuery : IQuery<bool>
    {
        [Required]
        [DataMember(Order = 1)] public string UserName { get; set; } = null!;

        [DataMember(Order = 2)] public TimeSpan TokenExpirationTimeSpan { get; set; }

        [Required]
        [DataMember(Order = 3)] public string VerificationToken { get; set; } = null!;
    }
}
