using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WebApp.Common.Infrastructure.Localization;

using static WebApp.Common.ModelConstants;

namespace WebApp.Service.Users;

[DataContract]
public record class CreateUserCommand : IKeyGeneratorCommand
{
    [Localized] private const string CommaNotAllowedErrorMessage = "The field {0} must contain no comma characters.";
    [Required, MaxLength(UserNameMaxLength), RegularExpression(@"^[^,]*$", ErrorMessage = CommaNotAllowedErrorMessage)]
    [DataMember(Order = 1)] public string UserName { get; init; } = null!;

    [Required, MaxLength(UserEmailMaxLength), EmailAddress]
    [DataMember(Order = 2)] public string Email { get; init; } = null!;

    [Required, Password(IgnoreIfServiceUnavailable = true)]
    [DataMember(Order = 3)] public string Password { get; init; } = null!;

    [DataMember(Order = 4)] public bool IsApproved { get; init; }

    [DataMember(Order = 5)] public bool CreateProfile { get; init; }

    [MaxLength(UserFirstNameMaxLength)]
    [DataMember(Order = 6)] public string? FirstName { get; init; }

    [MaxLength(UserLastNameMaxLength)]
    [DataMember(Order = 7)] public string? LastName { get; init; }

    public Action<ICommand, object>? OnKeyGenerated { get; set; }
}
