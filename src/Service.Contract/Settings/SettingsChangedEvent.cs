using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WebApp.Service.Settings
{
    [DataContract]
    public class SettingsChangedEvent : Event
    {
        [DataMember(Order = 1)] public long Version { get; set; }

        [DataMember(Order = 2)] public Dictionary<string, string?>? Data { get; set; }
    }
}
