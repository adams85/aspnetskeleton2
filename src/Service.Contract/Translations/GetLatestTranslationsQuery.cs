using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WebApp.Service.Translations
{
    [DataContract]
    public class GetLatestTranslationsQuery : IQuery<TranslationsChangedEvent[]?>
    {
    }
}
