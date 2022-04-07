using System.Runtime.Serialization;

namespace WebApp.Service.Settings
{
    [DataContract]
    public record class GetSettingQuery : IQuery<SettingData>
    {
        [DataMember(Order = 1)] public string Name { get; init; } = null!;
        [DataMember(Order = 2)] public bool IncludeDescription { get; init; }
    }
}
