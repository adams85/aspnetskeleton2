namespace POTools.Services.Extracting;

public class LocalizableTextInfo
{
    public int LineNumber { get; set; }
    public string Id { get; set; } = null!;
    public string? PluralId { get; set; }
    public string? ContextId { get; set; }
    public string? ExtractedComment { get; set; }
}
