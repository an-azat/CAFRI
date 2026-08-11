namespace CAFRI.Domain.Content;

public sealed class PublicationCategoryAssignment
{
    public Guid Id { get; set; }
    public Guid PublicationContentItemId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public PublicationContentItem PublicationContentItem { get; set; } = null!;
}
