using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.Service;
using WebApp.Service.Settings;
using WebApp.UI.Models;

namespace WebApp.UI.Areas.Dashboard.Models.Settings
{
    public class SettingListModel : ListModel<ListSettingsQuery, ListResult<SettingData>, SettingData>
    {
        public static LocalizedHtmlString GetDescription(SettingData data) =>
            new LocalizedHtmlString(data.Description, data.Description, false, data.DefaultValue, data.MinValue, data.MaxValue);
    }
}
