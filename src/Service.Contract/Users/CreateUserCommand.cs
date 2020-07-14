using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WebApp.Common.Infrastructure.Localization;

using static WebApp.Common.ModelConstants;

namespace WebApp.Service.Users
{
    [DataContract]
    public class CreateUserCommand : IKeyGeneratorCommand
    {
        [Required, MaxLength(UserNameMaxLength), RegularExpression(@"^[^,]*$", ErrorMessage = CommaNotAllowedErrorMessage)]
        [DataMember(Order = 1)] public string UserName { get; set; } = null!;

        [Required, MaxLength(UserEmailMaxLength), EmailAddress]
        [DataMember(Order = 2)] public string Email { get; set; } = null!;

        [Required, Password(IgnoreIfServiceUnavailable = true)]
        [DataMember(Order = 3)] public string Password { get; set; } = null!;

        [DataMember(Order = 4)] public bool IsApproved { get; set; }

        [DataMember(Order = 5)] public bool CreateProfile { get; set; }

        [MaxLength(UserFirstNameMaxLength)]
        [DataMember(Order = 6)] public string? FirstName { get; set; }

        [MaxLength(UserLastNameMaxLength)]
        [DataMember(Order = 7)] public string? LastName { get; set; }

        public Action<ICommand, object>? OnKeyGenerated { get; set; }

        [Localized] private const string CommaNotAllowedErrorMessage = "The field {0} must contain no comma characters.";
    }
}
