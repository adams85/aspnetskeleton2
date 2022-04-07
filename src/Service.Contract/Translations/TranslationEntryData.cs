using System.Runtime.Serialization;
using ProtoBuf;

namespace WebApp.Service.Translations
{
    [DataContract]
    [ProtoInclude(11, typeof(Singular))]
    [ProtoInclude(12, typeof(Plural))]
    public abstract record class TranslationEntryData
    {
        [DataMember(Order = 1)] public string Id { get; init; } = null!;
        [DataMember(Order = 2)] public string? PluralId { get; init; }
        [DataMember(Order = 3)] public string? ContextId { get; init; }

        [DataContract]
        public record class Singular : TranslationEntryData
        {
            [DataMember(Order = 1)] public string Translation { get; init; } = null!;
        }

        [DataContract]
        public record class Plural : TranslationEntryData
        {
            [DataMember(Order = 1)] public string[]? Translations { get; init; }
        }
    }
}
