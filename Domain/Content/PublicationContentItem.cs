namespace CAFRI.Domain.Content;

public sealed class PublicationContentItem
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string CountryLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Meta { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string PublishedDate { get; set; } = string.Empty;
    public string ReadingTime { get; set; } = string.Empty;
    public string AuthorLabel { get; set; } = string.Empty;
    public string DocumentLabel { get; set; } = string.Empty;
    public string HeroVisualClassName { get; set; } = string.Empty;
    public string? HeroImageUrl { get; set; }
    public string? GalleryJson { get; set; }
    public string? PdfDownloadUrl { get; set; }
    public string? DetailsJson { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ICollection<PublicationActionEntry> ActionEntries { get; set; } = [];
    public ICollection<PublicationHighlightEntry> HighlightEntries { get; set; } = [];
    public ICollection<PublicationFindingEntry> FindingEntries { get; set; } = [];
    public ICollection<PublicationArticleSection> Sections { get; set; } = [];
    public ICollection<PublicationDocumentEntry> DocumentEntries { get; set; } = [];
    public ICollection<PublicationRelatedLink> RelatedLinks { get; set; } = [];
    public ICollection<PublicationInfoEntry> InfoEntries { get; set; } = [];
}
