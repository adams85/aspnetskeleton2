using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public class ChangePasswordCommand : ICommand, IValidatableObject
    {
        [Required]
        [DataMember(Order = 1)] public string UserName { get; set; } = null!;

        [Required, Password(IgnoreIfServiceUnavailable = true)]
        [DataMember(Order = 2)] public string NewPassword { get; set; } = null!;

        [DataMember(Order = 3)] public bool Verify { get; set; }

        [DataMember(Order = 4)] public string? VerificationToken { get; set; }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            if (Verify)
                yield return this.RequireMember(VerificationToken, nameof(VerificationToken), validationContext);
        }
    }
}
