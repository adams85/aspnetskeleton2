using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Mailing.Users
{
    [DataContract]
    public record class PasswordResetMailModel : MailModel
    {
        public const string AssociatedMailType = "PasswordReset";

        public override string MailType => AssociatedMailType;

        [DataMember(Order = 1)] public string? Name { get; init; }

        [Required]
        [DataMember(Order = 2)] public string UserName { get; init; } = null!;

        [Required]
        [DataMember(Order = 3)] public string Email { get; init; } = null!;

        [Required]
        [DataMember(Order = 4)] public string VerificationToken { get; init; } = null!;

        [DataMember(Order = 5)] public DateTime VerificationTokenExpirationDate { get; init; }
    }
}
