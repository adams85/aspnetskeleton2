using POTools.Helpers;

namespace POTools.Services.Extracting;

public class LocalizableTextInfo
{
    public int LineNumber { get; set; }

    private string _id = null!;
    public string Id
    {
        get => _id;
        set => _id = value.NormalizeNewLines();
    }

    private string? _pluralId;
    public string? PluralId
    {
        get => _pluralId;
        set => _pluralId = value.NormalizeNewLines();
    }

    private string? _contextId;
    public string? ContextId
    {
        get => _contextId;
        set => _contextId = value.NormalizeNewLines();
    }

    public string? Translation { get; set; }

    public string? ExtractedComment { get; set; }
}
