namespace CAFRI.Domain.Content;

public sealed class IntelligenceContentItem
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string PublishedLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? HeroImageUrl { get; set; }
    public string? DetailsJson { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ICollection<IntelligenceArticleSection> Sections { get; set; } = [];
    public ICollection<IntelligenceKeyChangeEntry> KeyChangeEntries { get; set; } = [];
    public ICollection<IntelligenceImpactEntry> ImpactEntries { get; set; } = [];
    public ICollection<IntelligenceTimelineEntry> TimelineEntries { get; set; } = [];
    public ICollection<IntelligenceDocumentInfoEntry> DocumentInfoEntries { get; set; } = [];
    public ICollection<IntelligenceOfficialDocumentEntry> OfficialDocuments { get; set; } = [];
    public ICollection<IntelligenceRelatedLink> RelatedLinks { get; set; } = [];
    public ICollection<IntelligenceStatusEntry> StatusEntries { get; set; } = [];
    public ICollection<IntelligenceHighlightEntry> HighlightEntries { get; set; } = [];
}
