using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WebApp.Service.Settings;

[DataContract]
public record class SettingsChangedEvent : Event
{
    [DataMember(Order = 1)] public long Version { get; init; }

    [DataMember(Order = 2)] public Dictionary<string, string?>? Data { get; init; }
}
