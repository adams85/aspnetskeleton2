using System.Runtime.Serialization;
using ProtoBuf;

namespace WebApp.Service.Translations
{
    [DataContract]
    [ProtoInclude(11, typeof(Singular))]
    [ProtoInclude(12, typeof(Plural))]
    public abstract class TranslationEntryData
    {
        [DataMember(Order = 1)] public string Id { get; set; } = null!;
        [DataMember(Order = 2)] public string? PluralId { get; set; }
        [DataMember(Order = 3)] public string? ContextId { get; set; }

        [DataContract]
        public class Singular : TranslationEntryData
        {
            [DataMember(Order = 1)] public string Translation { get; set; } = null!;
        }

        [DataContract]
        public class Plural : TranslationEntryData
        {
            [DataMember(Order = 1)] public string[]? Translations { get; set; }
        }
    }
}
