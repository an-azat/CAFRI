namespace CAFRI.Domain.Content;

public sealed class IntelligenceKeyChangeEntry
{
    public Guid Id { get; set; }
    public Guid IntelligenceContentItemId { get; set; }
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public IntelligenceContentItem IntelligenceContentItem { get; set; } = null!;
}
