namespace CAFRI.Domain.Content;

public sealed class IntelligenceStatusEntry
{
    public Guid Id { get; set; }
    public Guid IntelligenceContentItemId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsStatus { get; set; }
    public int DisplayOrder { get; set; }

    public IntelligenceContentItem IntelligenceContentItem { get; set; } = null!;
}
