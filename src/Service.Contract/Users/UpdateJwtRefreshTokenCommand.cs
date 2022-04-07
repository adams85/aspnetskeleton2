using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public record class UpdateJwtRefreshTokenCommand : IKeyGeneratorCommand, IValidatableObject
    {
        [Required]
        [DataMember(Order = 1)] public string UserName { get; init; } = null!;

        [DataMember(Order = 2)] public TimeSpan TokenExpirationTimeSpan { get; init; }

        [DataMember(Order = 3)] public bool Verify { get; init; }

        [DataMember(Order = 4)] public string? VerificationToken { get; init; }

        public Action<ICommand, object>? OnKeyGenerated { get; set; }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            if (Verify)
                yield return this.RequireMember(VerificationToken, nameof(VerificationToken), validationContext);
        }
    }
}
