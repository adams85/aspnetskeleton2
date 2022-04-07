using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WebApp.Common.Infrastructure.Localization;

using static WebApp.Common.ModelConstants;

namespace WebApp.Service.Roles;

[DataContract]
public record class CreateRoleCommand : IKeyGeneratorCommand
{
    [Localized] private const string CommaNotAllowedErrorMessage = "The field {0} must contain no comma characters.";
    [Required, MaxLength(RoleNameMaxLength), RegularExpression(@"^[^,]*$", ErrorMessage = CommaNotAllowedErrorMessage)]
    [DataMember(Order = 1)] public string RoleName { get; init; } = null!;

    [MaxLength(RoleDescriptionMaxLength)]
    [DataMember(Order = 2)] public string? Description { get; init; }

    public Action<ICommand, object>? OnKeyGenerated { get; set; }
}
