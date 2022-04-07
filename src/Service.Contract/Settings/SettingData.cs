using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WebApp.Common.Infrastructure.Localization;

namespace WebApp.Service.Settings;

[Display(Name = DisplayName)]
[DataContract]
public record class SettingData
{
    [Localized] private const string DisplayName = "Setting";

    [Localized] private const string NameDisplayName = "Code";
    [Display(Name = NameDisplayName)]
    [DataMember(Order = 1)] public string Name { get; init; } = null!;

    [Localized] private const string ValueDisplayName = "Value";
    [Display(Name = ValueDisplayName)]
    [DataMember(Order = 2)] public string? Value { get; init; }

    [DataMember(Order = 3)] public string? DefaultValue { get; init; }

    [DataMember(Order = 4)] public string? MinValue { get; init; }

    [DataMember(Order = 5)] public string? MaxValue { get; init; }

    [Localized] private const string DescriptionDisplayName = "Description";
    [Display(Name = DescriptionDisplayName)]
    [DataMember(Order = 6)] public string? Description { get; init; }

    public UpdateSettingCommand ToUpdateCommand() => new UpdateSettingCommand
    {
        Name = this.Name,
        Value = this.Value
    };
}
