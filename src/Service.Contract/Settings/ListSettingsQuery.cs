using System.Runtime.Serialization;

namespace WebApp.Service.Contract.Settings
{
    [DataContract]
    public class ListSettingsQuery : ListQuery, IQuery<ListResult<SettingData>>
    {
    }
}
