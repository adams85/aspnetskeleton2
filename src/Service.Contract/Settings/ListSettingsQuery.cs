﻿using System.Runtime.Serialization;

namespace WebApp.Service.Settings
{
    [DataContract]
    public class ListSettingsQuery : ListQuery, IQuery<ListResult<SettingData>>
    {
    }
}
