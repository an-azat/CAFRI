namespace CAFRI.Domain.Content;

public sealed class IntelligenceRelatedLink
{
    public Guid Id { get; set; }
    public Guid IntelligenceContentItemId { get; set; }
    public string LinkType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string? Url { get; set; }
    public int DisplayOrder { get; set; }

    public IntelligenceContentItem IntelligenceContentItem { get; set; } = null!;
}
