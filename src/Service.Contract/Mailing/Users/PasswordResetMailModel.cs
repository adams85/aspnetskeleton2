using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Mailing.Users
{
    [DataContract]
    public class PasswordResetMailModel : MailModel
    {
        public static readonly string AssociatedMailType = "PasswordReset";

        public override string MailType => AssociatedMailType;

        [DataMember(Order = 1)] public string? Name { get; set; }

        [Required]
        [DataMember(Order = 2)] public string UserName { get; set; } = null!;

        [Required]
        [DataMember(Order = 3)] public string Email { get; set; } = null!;

        [Required]
        [DataMember(Order = 4)] public string VerificationToken { get; set; } = null!;

        [DataMember(Order = 5)] public DateTime VerificationTokenExpirationDate { get; set; }
    }
}
