namespace CAFRI.Domain.Content;

public sealed class IntelligenceHighlightEntry
{
    public Guid Id { get; set; }
    public Guid IntelligenceContentItemId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public IntelligenceContentItem IntelligenceContentItem { get; set; } = null!;
}
