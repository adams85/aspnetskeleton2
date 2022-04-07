using System.Runtime.Serialization;

namespace WebApp.Service.Settings
{
    [DataContract]
    public record class GetLatestSettingsQuery : IQuery<SettingsChangedEvent>
    {
    }
}
