namespace CAFRI.Domain.Content;

public sealed class PublicationActionEntry
{
    public Guid Id { get; set; }
    public Guid PublicationContentItemId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public PublicationContentItem PublicationContentItem { get; set; } = null!;
}
