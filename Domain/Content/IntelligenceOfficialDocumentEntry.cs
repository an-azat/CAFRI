namespace CAFRI.Domain.Content;

public sealed class IntelligenceOfficialDocumentEntry
{
    public Guid Id { get; set; }
    public Guid IntelligenceContentItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Meta { get; set; } = string.Empty;
    public string? DownloadUrl { get; set; }
    public int DisplayOrder { get; set; }

    public IntelligenceContentItem IntelligenceContentItem { get; set; } = null!;
}
