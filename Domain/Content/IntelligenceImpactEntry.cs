namespace CAFRI.Domain.Content;

public sealed class IntelligenceImpactEntry
{
    public Guid Id { get; set; }
    public Guid IntelligenceContentItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImpactLabel { get; set; } = string.Empty;
    public string ImpactTone { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public IntelligenceContentItem IntelligenceContentItem { get; set; } = null!;
}
