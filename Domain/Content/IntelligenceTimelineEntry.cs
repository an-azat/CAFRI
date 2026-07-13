namespace CAFRI.Domain.Content;

public sealed class IntelligenceTimelineEntry
{
    public Guid Id { get; set; }
    public Guid IntelligenceContentItemId { get; set; }
    public string DateLabel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public IntelligenceContentItem IntelligenceContentItem { get; set; } = null!;
}
