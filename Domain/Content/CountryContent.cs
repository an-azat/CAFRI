namespace CAFRI.Domain.Content;

public sealed class CountryContent
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Capital { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Gdp { get; set; } = string.Empty;
    public string Population { get; set; } = string.Empty;
    public string BankAssets { get; set; } = string.Empty;
    public string HeroClassName { get; set; } = string.Empty;
    public string? HeroGalleryJson { get; set; }
    public string DetailsJson { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
