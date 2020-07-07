using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WebApp.Common.Infrastructure.Localization;

using static WebApp.Common.ModelConstants;

namespace WebApp.Service.Roles
{
    [DataContract]
    public class CreateRoleCommand : IKeyGeneratorCommand
    {
        [Required, MaxLength(RoleNameMaxLength), RegularExpression(@"^[^,]*$", ErrorMessage = CommaNotAllowedErrorMessage)]
        [DataMember(Order = 1)] public string RoleName { get; set; } = null!;

        [MaxLength(RoleDescriptionMaxLength)]
        [DataMember(Order = 2)] public string? Description { get; set; }

        public Action<ICommand, object>? OnKeyGenerated { get; set; }

        [Localized] private const string CommaNotAllowedErrorMessage = "The field {0} must contain no comma characters.";
    }
}
