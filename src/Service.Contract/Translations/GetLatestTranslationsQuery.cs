using System.Runtime.Serialization;

namespace WebApp.Service.Translations
{
    [DataContract]
    public record class GetLatestTranslationsQuery : IQuery<TranslationsChangedEvent[]?>
    {
    }
}
