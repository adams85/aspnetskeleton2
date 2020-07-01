using System.Runtime.Serialization;

namespace WebApp.Service.Translations
{
    [DataContract]
    public class TranslationCatalogData
    {
        [DataMember(Order = 1)] public int PluralFormCount { get; set; }
        [DataMember(Order = 2)] public string? PluralFormSelector { get; set; }

        [DataMember(Order = 3)] public TranslationEntryData[]? Entries { get; set; }
    }
}
