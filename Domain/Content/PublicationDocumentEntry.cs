namespace CAFRI.Domain.Content;

public sealed class PublicationDocumentEntry
{
    public Guid Id { get; set; }
    public Guid PublicationContentItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Meta { get; set; } = string.Empty;
    public string? DownloadUrl { get; set; }
    public int DisplayOrder { get; set; }

    public PublicationContentItem PublicationContentItem { get; set; } = null!;
}
