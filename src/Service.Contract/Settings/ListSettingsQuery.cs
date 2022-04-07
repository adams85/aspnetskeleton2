using System.Runtime.Serialization;

namespace WebApp.Service.Settings;

[DataContract]
public record class ListSettingsQuery : ListQuery<ListResult<SettingData>>
{
    [DataMember(Order = 1)] public string? NamePattern { get; init; }
    [DataMember(Order = 2)] public string? ValuePattern { get; init; }
    [DataMember(Order = 3)] public string? DescriptionPattern { get; init; }
}
