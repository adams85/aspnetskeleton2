using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WebApp.Service.Settings
{
    [DataContract]
    public class GetCachedSettingsQuery : IQuery<Dictionary<string, string?>>
    {
    }
}
