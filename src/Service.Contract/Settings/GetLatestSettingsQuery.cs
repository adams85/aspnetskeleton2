using System.Runtime.Serialization;

namespace WebApp.Service.Settings
{
    [DataContract]
    public class GetLatestSettingsQuery : IQuery<SettingsChangedEvent>
    {
    }
}
