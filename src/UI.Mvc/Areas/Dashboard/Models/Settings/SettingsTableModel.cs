using WebApp.Service;
using WebApp.Service.Settings;
using WebApp.UI.Models;

namespace WebApp.UI.Areas.Dashboard.Models.Settings
{
    public class SettingsTableModel : DataTableModel<ListSettingsQuery, ListResult<SettingData>, SettingData>
    {
    }
}
