using System.Runtime.Serialization;

namespace WebApp.Service.Settings
{
    [DataContract]
    public class SettingsChangedEvent : Event { }
}
