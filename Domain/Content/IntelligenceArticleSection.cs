namespace CAFRI.Domain.Content;

public sealed class IntelligenceArticleSection
{
    public Guid Id { get; set; }
    public Guid IntelligenceContentItemId { get; set; }
    public string SectionKey { get; set; } = string.Empty;
    public string Heading { get; set; } = string.Empty;
    public string? Body { get; set; }
    public int DisplayOrder { get; set; }

    public IntelligenceContentItem IntelligenceContentItem { get; set; } = null!;
}
