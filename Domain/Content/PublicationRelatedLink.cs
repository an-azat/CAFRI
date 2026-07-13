namespace CAFRI.Domain.Content;

public sealed class PublicationRelatedLink
{
    public Guid Id { get; set; }
    public Guid PublicationContentItemId { get; set; }
    public string LinkType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Meta { get; set; } = string.Empty;
    public string? Url { get; set; }
    public int DisplayOrder { get; set; }

    public PublicationContentItem PublicationContentItem { get; set; } = null!;
}
