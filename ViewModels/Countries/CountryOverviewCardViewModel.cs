using CAFRI.ViewModels.Map;

namespace CAFRI.ViewModels.Countries;

public sealed class CountryOverviewCardViewModel
{
    public required string Code { get; init; }

    public required string Name { get; init; }

    public required string Capital { get; init; }

    public required string Summary { get; init; }

    public required string Gdp { get; init; }

    public required string Population { get; init; }

    public required string BankAssets { get; init; }

    public required string Slug { get; init; }

    public required string HeroClassName { get; init; }

    public string HeroImageUrl { get; init; } = string.Empty;

    public string IndicatorLabel { get; init; } = "Indicator";

    public string IndicatorValue { get; init; } = "No data";

    public IReadOnlyList<CountryMetricViewModel> Metrics { get; init; } = [];

    public IReadOnlyList<MapCountryIndicatorValueViewModel> Indicators { get; init; } = [];

    public string AccentClass => Code switch
    {
        "KZ" => "country-badge--kz",
        "UZ" => "country-badge--uz",
        "KG" => "country-badge--kg",
        "TJ" => "country-badge--tj",
        "TM" => "country-badge--tm",
        _ => "country-badge--default"
    };

    // Centralized here so the country -> flag icon mapping exists in exactly one place
    // (it used to be duplicated in Views/Home/Index.cshtml as a local function).
    public string FlagIconUrl => Code switch
    {
        "KZ" => "/uploads/img/icons8-казахстан-48.png",
        "UZ" => "/uploads/img/icons8-узбекистан-48.png",
        "KG" => "/uploads/img/icons8-киргизия-48.png",
        "TJ" => "/uploads/img/icons8-таджикистан-48.png",
        "TM" => "/uploads/img/icons8-туркменистан-циркуляр-48.png",
        _ => string.Empty
    };

    public string Url => $"/countries/{Slug}";

    // Short "key theme" tag derived from Summary rather than a separate authored
    // field: Summary sentences follow a "<economy type> with <theme>." pattern
    // (see App_Data seed data), so the text after the last "with" reads as a
    // reasonable one-line focus area. Falls back to the full Summary if that
    // pattern isn't present (e.g. a custom admin-edited summary).
    public string Focus
    {
        get
        {
            const string marker = " with ";
            var index = Summary.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);
            var text = (index >= 0 ? Summary[(index + marker.Length)..] : Summary).Trim().TrimEnd('.');
            return text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];
        }
    }
}
