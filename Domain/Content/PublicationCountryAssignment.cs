namespace CAFRI.Domain.Content;

public sealed class PublicationCountryAssignment
{
    public Guid Id { get; set; }
    public Guid PublicationContentItemId { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string CountryLabel { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public PublicationContentItem PublicationContentItem { get; set; } = null!;
}
