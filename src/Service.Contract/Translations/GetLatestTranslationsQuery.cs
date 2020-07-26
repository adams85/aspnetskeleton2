using System.Runtime.Serialization;

namespace WebApp.Service.Translations
{
    [DataContract]
    public class GetLatestTranslationsQuery : IQuery<TranslationsChangedEvent[]?>
    {
    }
}
