using POTools.Helpers;

namespace POTools.Services.Extracting;

public class LocalizableTextInfo
{
    public int LineNumber { get; set; }

    public string Id
    {
        get;
        set => field = value.NormalizeNewLines();
    } = null!;

    public string? PluralId
    {
        get;
        set => field = value.NormalizeNewLines();
    }

    public string? ContextId
    {
        get;
        set => field = value.NormalizeNewLines();
    }

    public string? Translation { get; set; }

    public string? ExtractedComment { get; set; }
}
