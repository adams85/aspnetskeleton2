using System.Runtime.Serialization;

namespace WebApp.Service.Translations
{
    [DataContract]
    public class TranslationsChangedEvent : Event
    {
        [DataMember(Order = 1)] public string Location { get; set; } = null!;
        [DataMember(Order = 2)] public string Culture { get; set; } = null!;

        [DataMember(Order = 3)] public long Version { get; set; }

        [DataMember(Order = 4)] public TranslationCatalogData? Data { get; set; }
    }
}
