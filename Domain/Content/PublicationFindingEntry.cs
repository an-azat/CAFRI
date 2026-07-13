namespace CAFRI.Domain.Content;

public sealed class PublicationFindingEntry
{
    public Guid Id { get; set; }
    public Guid PublicationContentItemId { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public PublicationContentItem PublicationContentItem { get; set; } = null!;
}
