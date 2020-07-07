namespace POTools.Services.Extracting
{
    public class CSharpTextExtractorSettings
    {
        public static readonly string DefaultLocalizedAttributeName = "Localized";
        public static readonly string DefaultLocalizedAttributePluralIdArgName = "PluralId";
        public static readonly string DefaultLocalizedAttributeContextIdArgName = "ContextId";

        public static readonly string DefaultLocalizerMemberName = "T";

        public static readonly string DefaultPluralTypeName = "Plural";
        public static readonly string DefaultPluralFactoryMemberName = "From";

        public static readonly string DefaultTextContextTypeName = "TextContext";
        public static readonly string DefaultTextContextFactoryMemberName = "From";

        public string? LocalizedAttributeName { get; set; }
        public string? LocalizedAttributePluralIdArgName { get; set; }
        public string? LocalizedAttributeContextIdArgName { get; set; }

        public string? LocalizerMemberName { get; set; }

        public string? PluralTypeName { get; set; }
        public string? PluralFactoryMemberName { get; set; }

        public string? TextContextTypeName { get; set; }
        public string? TextContextFactoryMemberName { get; set; }
    }
}
