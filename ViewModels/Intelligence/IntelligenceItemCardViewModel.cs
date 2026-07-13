namespace CAFRI.ViewModels.Intelligence;

public sealed class IntelligenceItemCardViewModel
{
    public required string Slug { get; init; }

    public required string Category { get; init; }

    public required string CountryCode { get; init; }

    public required string CountryName { get; init; }

    public required string PublishedLabel { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string Source { get; init; }

    public string Url => $"/intelligence/{Slug}";

    public string CountryAccentClass => CountryCode switch
    {
        "KZ" => "country-badge--kz",
        "UZ" => "country-badge--uz",
        "KG" => "country-badge--kg",
        "TJ" => "country-badge--tj",
        "TM" => "country-badge--tm",
        _ => "country-badge--default"
    };
}
