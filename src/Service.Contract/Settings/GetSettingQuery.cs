using System.Runtime.Serialization;

namespace WebApp.Service.Settings
{
    [DataContract]
    public class GetSettingQuery : IQuery<SettingData>
    {
        [DataMember(Order = 1)] public string Name { get; set; } = null!;
        [DataMember(Order = 2)] public bool IncludeDescription { get; set; }
    }
}
