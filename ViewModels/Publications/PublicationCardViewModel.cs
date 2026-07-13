namespace CAFRI.ViewModels.Publications;

public sealed class PublicationCardViewModel
{
    public required string Type { get; init; }

    public required string CountryCode { get; init; }

    public required string CountryLabel { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string Meta { get; init; }

    public required string Slug { get; init; }

    public string Url => $"/publications/{Slug}";

    public string AccentClass => CountryCode switch
    {
        "KZ" => "country-badge--kz",
        "UZ" => "country-badge--uz",
        "KG" => "country-badge--kg",
        "TJ" => "country-badge--tj",
        "TM" => "country-badge--tm",
        _ => "country-badge--default"
    };
}
