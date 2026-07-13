namespace CAFRI.Domain.Content;

public sealed class CountryIndicator
{
    public Guid Id { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string LatestValue { get; set; } = string.Empty;
    public string YearLabel { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
