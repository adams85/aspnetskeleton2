using System.Runtime.Serialization;

namespace WebApp.Service.Translations;

[DataContract]
public record class TranslationCatalogData
{
    [DataMember(Order = 1)] public int PluralFormCount { get; init; }
    [DataMember(Order = 2)] public string? PluralFormSelector { get; init; }

    [DataMember(Order = 3)] public TranslationEntryData[]? Entries { get; set; }
}
