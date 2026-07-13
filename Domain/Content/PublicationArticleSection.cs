namespace CAFRI.Domain.Content;

public sealed class PublicationArticleSection
{
    public Guid Id { get; set; }
    public Guid PublicationContentItemId { get; set; }
    public string SectionKey { get; set; } = string.Empty;
    public string Heading { get; set; } = string.Empty;
    public string ParagraphsText { get; set; } = string.Empty;
    public string BulletPointsText { get; set; } = string.Empty;
    public string? CalloutTone { get; set; }
    public string? CalloutLabel { get; set; }
    public string? CalloutTitle { get; set; }
    public string? CalloutDescription { get; set; }
    public int DisplayOrder { get; set; }

    public PublicationContentItem PublicationContentItem { get; set; } = null!;
}
