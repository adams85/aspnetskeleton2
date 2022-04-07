using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Settings;

[DataContract]
public record class UpdateSettingCommand : ICommand
{
    [Required]
    [DataMember(Order = 1)] public string Name { get; init; } = null!;

    [DataMember(Order = 2)] public string? Value { get; init; }
}
