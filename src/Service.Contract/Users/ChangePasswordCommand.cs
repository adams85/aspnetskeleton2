using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public record class ChangePasswordCommand : ICommand, IValidatableObject
    {
        [Required]
        [DataMember(Order = 1)] public string UserName { get; init; } = null!;

        [Required, Password(IgnoreIfServiceUnavailable = true)]
        [DataMember(Order = 2)] public string NewPassword { get; init; } = null!;

        [DataMember(Order = 3)] public bool Verify { get; init; }

        [DataMember(Order = 4)] public string? VerificationToken { get; init; }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            if (Verify)
                yield return this.RequireMember(VerificationToken, nameof(VerificationToken), validationContext);
        }
    }
}
