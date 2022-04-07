using System.Runtime.Serialization;

namespace WebApp.Service.Translations;

[DataContract]
public record class TranslationsChangedEvent : Event
{
    [DataMember(Order = 1)] public string Location { get; init; } = null!;
    [DataMember(Order = 2)] public string Culture { get; init; } = null!;

    [DataMember(Order = 3)] public long Version { get; init; }

    [DataMember(Order = 4)] public TranslationCatalogData? Data { get; init; }
}
