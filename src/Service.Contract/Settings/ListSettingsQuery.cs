using System.Runtime.Serialization;

namespace WebApp.Service.Settings
{
    [DataContract]
    public class ListSettingsQuery : ListQuery, IQuery<ListResult<SettingData>>
    {
        [DataMember(Order = 1)] public string? NamePattern { get; set; }
        [DataMember(Order = 2)] public string? ValuePattern { get; set; }
        [DataMember(Order = 3)] public string? DescriptionPattern { get; set; }
    }
}
